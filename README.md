# Atividade-prog-web-rabbit

O projeto executa RabbitMQ, um consumer .NET e um producer Python:

```powershell
docker compose up -d --build
```

O producer escolhe aleatoriamente um pedido pronto e o envia a cada 5 segundos.
As opções incluem:

- compra coerente;
- produto vazio;
- quantidade inválida;
- valor inválido;
- produto não cadastrado;
- valor divergente.

Para acompanhar as mensagens e validações:

```powershell
docker compose logs -f producer consumer
```

A API fica disponível em http://localhost:8081 e o painel do RabbitMQ em
http://localhost:15672 (`admin` / `admin`).

Para encerrar os serviços:

```powershell
docker compose down
```
