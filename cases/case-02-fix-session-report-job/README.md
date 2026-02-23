# Conclusão Final — Case 2

## (a)

O método `GetTimeBucketsDictionary()` cria um `Dictionary<string,int>`, representando **cada segundo entre** do dia especificado, no caso desse teste: 22/02/2021.

- Chave: horário no formato `"HH:mm:ss"`
- Valor inicial: `0`

## (b)

O `output.csv` contém a **quantidade de mensagens `IN` com `35=D` por segundo** dentro desse intervalo.

Formato de cada linha:

- Segundos sem mensagens → `0`
- Segundos com múltiplas mensagens → valor correspondente à contagem
- O arquivo desse teste terá inicialmente **39.601 linhas**, uma para cada segundo do intervalo.