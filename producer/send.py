import json
import pika


print("--- Cadastro de Pagamento ---")
nome = input("Digite o Nome: ")
valor = input("Digite o Valor: ")
quantidade = input("Digite a Quantidade: ")


dados_pagamento = {
    "nome": nome,
    "valor": valor,
    "quantidade": quantidade,
}


mensagem = json.dumps(dados_pagamento)


connection = pika.BlockingConnection(pika.ConnectionParameters("localhost"))
channel = connection.channel()


channel.queue_declare(
    queue="pagamento", durable=True, arguments={"x-queue-type": "quorum"}
)


channel.basic_publish(exchange="", routing_key="pagamento", body=mensagem)

print(f" [x] Enviado com sucesso: {mensagem}")


connection.close()