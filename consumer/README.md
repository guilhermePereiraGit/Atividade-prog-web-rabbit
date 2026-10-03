# Consumidor de produtos com RabbitMQ

API .NET 10 que consome compras da fila `produtos.recebidos` e compara o produto e o valor unitário com um catálogo SQLite. Inconsistências são publicadas na fila `produtos.avisos`.

## Executar

É necessário ter Docker com Docker Compose instalado.

```powershell
docker compose up --build
```

Serviços disponíveis:

- API: http://localhost:8081
- RabbitMQ Management: http://localhost:15672 (`admin` / `admin`)

O catálogo é recriado ao iniciar a aplicação:

| Produto | Valor unitário |
| --- | ---: |
| Monitor | 250,00 |
| Teclado | 100,00 |
| Mouse | 50,00 |
| GoPro | 300,00 |
| Fone | 150,00 |

## Publicar uma mensagem

Com os containers em execução, use o PowerShell:

```powershell
$message = @{
    nomeProduto = "Mouse"
    quantidade = 2
    valorUnitario = 55.00
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

O exemplo informa um total de R$ 110,00, mas o catálogo espera R$ 100,00. Veja o aviso nos logs:

```powershell
docker compose logs -f consumer
```

O mesmo aviso em JSON fica disponível na fila `produtos.avisos`, acessível pela interface do RabbitMQ. Produtos inexistentes também geram aviso. Mensagens válidas são confirmadas manualmente; JSON inválido é descartado e falhas transitórias devolvem a mensagem à fila.

## Configuração

As opções ficam em `WebApplication1/appsettings.json`. Em containers ou produção, cada valor pode ser sobrescrito com variáveis como `RabbitMq__HostName`, `RabbitMq__UserName` e `RabbitMq__Password`.