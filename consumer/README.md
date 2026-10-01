# Consumidor de produtos com RabbitMQ

API .NET 10 que consome produtos da fila `produtos.recebidos` e compara quantidade e valor unitário com um catálogo SQLite em memória. O SQLite cumpre neste projeto o mesmo papel que o H2 costuma cumprir em aplicações Java.

## Executar

É necessário ter Docker com Docker Compose instalado.

```powershell
docker compose up --build
```

Serviços disponíveis:

- API e catálogo: http://localhost:8080/produtos
- RabbitMQ Management: http://localhost:15672 (`guest` / `guest`)

O catálogo é recriado ao iniciar a aplicação:

| Produto | Quantidade | Valor unitário |
| --- | ---: | ---: |
| Notebook | 10 | 3500,00 |
| Mouse | 50 | 89,90 |
| Teclado | 30 | 199,90 |

## Publicar uma mensagem

Com os containers em execução, use o PowerShell:

```powershell
$message = @{
    nomeProduto = "Notebook"
    quantidade = 10
    valorUnitario = 3500.00
} | ConvertTo-Json -Compress

$body = @{
    properties = @{ delivery_mode = 2 }
    routing_key = "produtos.recebidos"
    payload = $message
    payload_encoding = "string"
} | ConvertTo-Json -Depth 5

$credential = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("guest:guest"))
Invoke-RestMethod `
    -Uri "http://localhost:15672/api/exchanges/%2F/amq.default/publish" `
    -Method Post `
    -Headers @{ Authorization = "Basic $credential" } `
    -ContentType "application/json" `
    -Body $body
```

Veja o resultado da comparação nos logs:

```powershell
docker compose logs -f consumer
```

Mensagens válidas são confirmadas manualmente. JSON inválido é descartado; falhas transitórias devolvem a mensagem à fila. Produtos desconhecidos geram um aviso e são confirmados.

## Configuração

As opções ficam em `WebApplication1/appsettings.json`. Em containers ou produção, cada valor pode ser sobrescrito com variáveis como `RabbitMq__HostName`, `RabbitMq__UserName` e `RabbitMq__Password`.