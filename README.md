## Introdução

Para fins comparativos, fizemos 2 soluções: Uma síncrona e outra assíncrona, e ao longo da apresentação haverão comparativos entre ambas, que servirão como apoio para a nossa conclusão final.
## Premissas

Para iniciar o desenho da arquitetura e o desenvolvimento do código, partimos de 4 premissas que guiaram nossas ideias:
- O melhor cenário é ter 2 estruturas de dados (uma pra *bid* e outra pra *ask*);
- Pegar a ordem oposta para tentar o *match* pode ser paralelo, pois ele só pega a o contrário da ordem recebida;
- Checar o preço no *Order Book* precisa ser atômico, pois é necessário acessar a estrutura de dados para consulta do *book* (se paralelizado, pode gerar *deadklocks*).
- O fazer a *trade* precisa ser atômico, pois precisamos considerar o *match* parcial, que torna necessário o acesso ao *Order Book*. 
## Arquitetura

Dada essa premissa, propomos a seguinte arquitetura: Duas *heaps* (`AskHeap` e `BidHeap`), desenvolvidas usando *PriorityQueue* ordenadas por um `Record`  contendo: preço, *timestamp* e *sequence*. Para o acesso às *heaps*, utilizamos 2 semáforos (um para cada *heap*), pois uma ordem que precisa acessar uma das *heaps* só precisa acessar a outra em caso de não dar match ou match parcial. Dividimos em 2 fases de execução:
- **Fase 1:** Trata uma ordem completa, sem considerar restos (ou seja, matchings parciais precisam da fase 2). Dessa forma, basta olhar para a *heap* oposta, não é preciso inserir na própria *heap*, otimizando custos. 
- **Fase 2:** Trata a *order* e seus restos. Essa efetivamente olha pra *heap* oposta, pega o resto da *order* e salva na *heap* própria. Ou seja, somente aqui há escrita no *Order Book*. 
Como critério de desempate ao timestamp, criamos um `Sequence`, que serve como um contador único da *engine*, incrementando toda vez que uma ordem chega ao *book*. Ele serve como um 3° critério de prioridade e, sem ele, a *heap* desempate de forma arbitrária.


Portanto, dado essas características, foi desenhada a seguinte arquitetura:

![[image.png]]

## Complexidade Algorítmica

Em ambas as soluções,  podemos avaliar a complexidade algorítmica em 3 momentos. Na solução síncrona:

| Intenção | Operação                                                                          | Custo        |
| -------- | --------------------------------------------------------------------------------- | ------------ |
| Busca    | `.Where() `percorre a lista inteira,<br>`.OrderBy().ThenBy()` ordena              | `O(n log n)` |
| Inserção | `List.Add` no fim do array                                                        | `O(1)`       |
| Remoção  | `List.Remove` faz busca linear (`IndexOf`) +<br>`Array.Copy` para fechar o buraco | `O(n)`       |

Já na solução assíncrona:

| Intenção                  | Operação              | Custo      |
| ------------------------- | --------------------- | ---------- |
| Buscar o melhor elemento  | `TryPeek`             | `O(1)`     |
| Inserção/Remoção na pilha | `Dequeue` / `Enqueue` | `O(log n)` |

A grande otimização esta na busca pelo melhor elemento, sendo uma busca de tempo constante. Uma vez que, em uma *heap* ordenada por prioridade, se o topo não dá match, ninguém dá.

## Gerenciamento de Estado

Cada *order* que vai ser inserida no *book* recebe um novo Id (a menos que a quantidade não tenha sido alterada), a *order* inicial nunca é reutilizada.  O `Priority` é um `record`, ou seja, imutável, copiado por valor. Por isso o *maker* parcialmente executado volta pra *heap* com a prioridade original e mantém o lugar exato na fila, em vez de "chegar de novo" atrás de quem veio depois.

## Paralelismo

Tivemos 2 momentos onde foram necessários operações atômicas:
- Acesso as *heaps*: Para não gerar concorrência entre *orders* o acesso a ambas as *heaps* precisa ser síncrono. Tanto na fase 1 que não considera restos (*lockando* apenas uma *heap*), quanto na fase 2 (*lockando* ambas as *heaps* para salvamento no *book*)
- Acesso ao *book*: Quando efetivamente vamos adicionar a trade feita na lista de trades e as *orders* nas respectivas listas de *orders*  processadas, precisamos fazer isso de forma atômica, utilizando *lock*, para não haver sobreposição de escritas. 

## Escalabilidade

Considerando apenas 1 *book*, há uma grande limitação em relação a melhorias propostas nessa solução, pois como vimos, poucas coisas são, de fato, paralelizáveis. Porém a implementação feita permite uma otimização do tempo considerando a arquitetura proposta, e dessa forma ao escalonar o único fator possível, que seria o número de orders simultâneas, o sistema continuaria com o mesmo comportamento, processando 2 *threads* simultaneamente na fase 1 (uma para *ask* e outra para *bid*). Já considerando o contexto de N *books*, é possível fazer um paralelismo com maior granularidade. Uma vez que cada *book* pode ser paralelo, e internamente com nossa implementação, também serem paralelos. 

## Performance e Resultados

Os testes mostraram que a implementação assíncrona que propomos superou a síncrona, provando que, para essa forma de implementação do *Order Book* é possível otimizar a solução quando usada essa estrutura de dados e essa forma de paralelizar o sistema. Abaixo segue o resultado comparativo dos testes:

Para executar os testes utilizando o Benchmark (que suprime o JIT e outras execuções do Visual Studio), basta rodar no terminal (na pasta Benchmark):
`$: dotnet run -c Release`

| Teste                         | Duração |
| ----------------------------- | ------- |
| *ChaosTestAsync_2*            | ~20.19 ms  |
| *ChaosTestAsync*              | ~22.83 ms  |
| *ChaosTestSyncLock*           | ~27.62 ms  |
| *ChaosTestSyncSemaphore*      | ~43.06 ms |

## Conclusão

Ao estudar diversas formas de desenhar a arquitetura dessa solução, percebemos que, para o desafio proposto, não existia solução perfeita. Qualquer solução tem seu ganho e sua perda, que devem ser considerados e colocados na balança para definir qual é a melhor abordagem do problema. Até mesmo soluções síncronas podem ser mais viáveis que soluções assíncronas, a depender da forma que for implementado. Em suma, esse exercício é importante para refletir sobre qual a melhor forma possível de pensar e desenvolver uma solução, apenas consultando documentações e colegas, sem uso assistido de IA. 
