### 📈Projeto de comunicação periodica de IVA ao portal das finanças

O presente projeto é destinado a comunicação periodica de Iva ao portal da finanças por meio de uma API fornecida pelo portal das finanças.
A aplicação também permite autenticação de contablista com plenos poderes declarativos para o cliente, bem como contablista sem plenos poderes declarativos para o cliente. Para informações mais detalhadas pode ler o ficheiro em PDF "Comunicacao_Declaracoes_Periodicas_de_IVA_a_AT".


### Algumas funcionalidades a destacar
Finanças IVA C#-API_ -> Form1.cs

***Funções***
- EncryptAT: A função tem como variáveis de entrada duas variáveis uma para a password, e o caminho do certificado, o principal objetivo da função é gerar a chave simetrica "AES", e com a mesma chave simetrica chamar outras funções para criptografar a password, a data, o nonce e a digest.

- CalculateSHA1Digest:

- EncryptAES: Esta função recebe uma string input, e uma chave simetrica AES, o principal objetivo da função é criptografar valores, para tal primeiro é criado o modo de criptofrafia onde foi usado ECB, e PKC7, depois é iniciado o processo de criptografia, onde é convertida a string para bytes, onde é criptografado os bytes, e no final são convertidos para base 64.

- EncryptRSA: Esta função recebe a chave simetrica, e a chave RSA que esta no certificado. O objetivo da função é criar uma chave simetrica para o Nonce com a RSA, começando por criptografalos e no final retornar os dados convertidos em base 64.

- GetDeclaration_: Esta função como valores o caminho do ficheiro da declaração de IVA, o principal objetivo da função é ler ficheiro da declaração, comprimir o ficheiro para zip, depois fazer dupla conversão para base64 do ficheiro, e retornar esse valor.

- SendFileWithCertificate:

<br/>

***Variáveis***
- StrEncryptPasswordAT: Variavel destinada a guardar a password criptografada. <br/>

- StrEncryptCreatedAT: Variavel destinada a guardar a data criptografada. <br/>

- StrEncryptNonceAT: Variavel destinada a guardar o Nonce criptografado.<br/>

- StrDigestAT: Variavel destinada a guardar a disgest criptografada. <br/>

***Links***

- Link da API de testes destinado a autenticação <br/>
https://servicos.portaldasfinancas.gov.pt:706/dpivaws/DeclaracaoPeriodicaIVAWebService

- Link da API de produção destinado a autenticação
https://servicos.portaldasfinancas.gov.pt:406/dpivaws/DeclaracaoPeriodicaIVAWebService

- Link destinado ao envio do ficheiro soap <br/>
https://servicos.portaldasfinancas.gov.pt/dpivaws/DeclaracaoPeriodicaIVAWebService/

- Site do E fatura <br/>
https://faturas.portaldasfinancas.gov.pt/

- Site das Finanças <br/>
https://www.portaldasfinancas.gov.pt/at/html/index.html
