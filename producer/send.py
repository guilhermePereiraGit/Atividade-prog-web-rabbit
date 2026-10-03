import json
import os
import random
import time

import pika


QUEUE_NAME = "produtos.recebidos"
SEND_INTERVAL_SECONDS = float(os.getenv("SEND_INTERVAL_SECONDS", "5"))
MOCK_ORDERS = (
    {"nomeProduto": "Mouse", "quantidade": 2, "valorUnitario": 50.0},
    {"nomeProduto": "Monitor", "quantidade": 1, "valorUnitario": 300.0},
    {"nomeProduto": "Webcam", "quantidade": 1, "valorUnitario": 100.0},
    {"nomeProduto": "Teclado", "quantidade": 0, "valorUnitario": 100.0},
    {"nomeProduto": "Fone", "quantidade": 3, "valorUnitario": 0.0},
    {"nomeProduto": "", "quantidade": 1, "valorUnitario": 50.0},
)


def create_connection():
    credentials = pika.PlainCredentials(
        os.getenv("RABBITMQ_USER", "admin"),
        os.getenv("RABBITMQ_PASSWORD", "admin"),
    )
    return pika.BlockingConnection(
        pika.ConnectionParameters(
            os.getenv("RABBITMQ_HOST", "localhost"),
            credentials=credentials,
        )
    )


def main():
    connection = create_connection()
    channel = connection.channel()
    channel.queue_declare(queue=QUEUE_NAME, durable=True)

    print("--- Gerador de pedidos iniciado ---", flush=True)

    try:
        while True:
            order = random.choice(MOCK_ORDERS)
            message = json.dumps(order)
            channel.basic_publish(
                exchange="",
                routing_key=QUEUE_NAME,
                body=message,
                properties=pika.BasicProperties(delivery_mode=2),
            )
            print(f"[x] Pedido enviado: {message}", flush=True)
            time.sleep(SEND_INTERVAL_SECONDS)
    except KeyboardInterrupt:
        print("\nGerador encerrado.")
    finally:
        if connection.is_open:
            connection.close()


if __name__ == "__main__":
    main()