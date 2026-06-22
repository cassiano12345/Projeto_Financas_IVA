### 📈Projeto de comunicação periodica de IVA ao portal das finanças

O presente projeto é destinado a comunicação periodica de Iva ao portal da finanças por meio de uma API fornecida pelo portal das finanças.
A aplicação também permite autenticação de contablista com plenos poderes declarativos para o cliente, bem como contablista sem plenos poderes declarativos para o cliente. Para informações mais detalhadas pode ler o ficheiro em PDF "Comunicacao_Declaracoes_Periodicas_de_IVA_a_AT".


### Algumas funcionalidades a destacar
Finanças IVA C#-API_ -> Form1.cs

***Funções***
- EncryptAT: A função tem como variáveis de entrada duas variáveis uma para a password, e o caminho da chave publica, o principal objetivo da função é gerar a chave simetrica "AES", e com a mesma chave simetrica chamar outras funções para criptografar a password, a data, o nonce e a digest.

- CalculateSHA1Digest: A função tem como objetivo calcular a Digest para o ficheiro SOAP, para tal a função recebe os valores, da password, da data atual, e a chave simetrica AES. Ao gerar a digest, a função concatena os valores de AES, da data, e da password, depois calcula o SHA-1 do valor concatenado, criptografa o SHA-1 gerado usando AES para tal é criado o modo de criptografia usando usado ECB, e PKC7, depois é iniciado o processo de criptografia, no final a função retorna o resultado em base64.

- EncryptAES: Esta função recebe uma string input, e uma chave simetrica AES, o principal objetivo da função é criptografar valores, para tal primeiro é criado o modo de criptofrafia onde foi usado ECB, e PKC7, depois é iniciado o processo de criptografia, onde é convertida a string para bytes, onde é criptografado os bytes, e no final são convertidos para base 64.

- EncryptRSA: Esta função recebe a chave simetrica, e a chave RSA que esta na chave publica. O objetivo da função é criar uma chave simetrica para o Nonce com a RSA, começando por criptografalos e no final retornar os dados convertidos em base 64.

- GetDeclaration_: Esta função como valores o caminho do ficheiro da declaração de IVA, o principal objetivo da função é ler ficheiro da declaração, comprimir o ficheiro para zip, depois fazer dupla conversão para base64 do ficheiro, e retornar esse valor.

- SendFileWithCertificate: A função tem como objetivo enviar a declaração de IVA, para tal a função recebe o ficheiro SOAP ja com a password, digest, e nonce criptografados, o link da API para fazer a autenticação, o link da ação SOAP na API, o caminho do certificado, e a password do certificado. O primeiro passo foi carregar o certificado com a senha, depois foi criado o header onde foi definido o metodo "POST", depois foi criado o envelope com os dados SOAP, foi convertido o envelope em bytes, o passo seguinte foi enviar os dados, e no final é recebida a resposta onde é possivel ver se esta tudo OK ou se a declaração tem algum erro de contablidade. 

<br/>

***Variáveis***
- StrEncryptPasswordAT: Variavel destinada a guardar a password criptografada. <br/>

- StrEncryptCreatedAT: Variavel destinada a guardar a data criptografada. <br/>

- StrEncryptNonceAT: Variavel destinada a guardar o Nonce criptografado.<br/>

- StrDigestAT: Variavel destinada a guardar a disgest criptografada. <br/>

***Imagens*** <br/>

***Página principal*** <br/>
A imagem a baixo mostra a página principal do programa, onde é possivel ver os campos do NIF, da password do contribuente, do lado direito é possivel ver também os campos do contablista certificado da empresa, e o campo para a sua password, mais em baixo é possivel ver o campo destinado a mostrar a resposta do servidor. 

<p align="center">
  <img src="Imagem da aplicação.png" alt="OpenMontage" width="700">
</p>

***Tipos de autenticação*** <br/>
A imagem a baixo mostra os tipos de autenticação possiveis na aplicação.

<p align="center">
  <img src="Tipos de autenticação.png" alt="OpenMontage" width="700">
</p>

***Links***

- Link da API de testes destinado a autenticação <br/>
https://servicos.portaldasfinancas.gov.pt:706/dpivaws/DeclaracaoPeriodicaIVAWebService

- Link da API de produção destinado a autenticação
https://servicos.portaldasfinancas.gov.pt:406/dpivaws/DeclaracaoPeriodicaIVAWebService

- Link da ação SOAP na API <br/>
https://servicos.portaldasfinancas.gov.pt/dpivaws/DeclaracaoPeriodicaIVAWebService/

- Site do E fatura <br/>
https://faturas.portaldasfinancas.gov.pt/

- Site das Finanças <br/>
https://www.portaldasfinancas.gov.pt/at/html/index.html

- Manual de integração de software <br/>
https://info.portaldasfinancas.gov.pt/pt/apoio_ao_contribuinte/Outras_entidades/Suporte_tecnologico/Webservice/IVA/Comunicacao_declaracoes_periodicas_IVA/Documents/Comunicacao_Declaracoes_Periodicas_de_IVA_a_AT.pdf
