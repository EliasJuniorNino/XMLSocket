# XMLSocket

**XMLSocket** é uma biblioteca simples em C# para comunicação entre cliente e servidor usando sockets TCP/IP. Permite o envio e recebimento de mensagens em formato texto (como XML) de forma fácil, com suporte a eventos e logging.

## ✨ Funcionalidades

- Comunicação via TCP (cliente e servidor)
- Envio e recebimento de mensagens em texto (ASCII/XML)
- Eventos para:
  - Conexão (`OnConnect`)
  - Recebimento de dados (`OnReceive`)
  - Envio de dados (`OnSend`)
  - Fechamento (`OnClose`)
- Log de eventos e exceções
- Operações assíncronas com threads

## 🚀 Como usar

### Clonar o repositório

```bash
git clone https://github.com/EliasJuniorNino/XMLSocket.git
```

## Exemplo básico
```c#
var socket = new XMLSocket();
socket.OnConnect += (args) => Console.WriteLine("Conectado!");
socket.OnReceive += (args) => Console.WriteLine("Recebido: " + args.XML);

socket.Connect("127.0.0.1", 2121);
socket.SendText("<mensagem>Olá, servidor!</mensagem>");
```
