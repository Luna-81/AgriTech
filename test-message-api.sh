#!/bin/bash

echo "========================================="
echo "  使用 HTTP API 测试消息流"
echo "========================================="

# 1. 检查当前队列状态
echo -e "\n1. 当前队列状态:"
curl -s -u guest:guest \
    http://localhost:15672/api/queues/%2F/sensor-registered-queue \
    | jq '{name, messages_ready, messages_unacknowledged, consumers, messages}'

# 2. 发送测试消息
echo -e "\n2. 发送测试消息..."
curl -s -u guest:guest -X POST \
    http://localhost:15672/api/exchanges/%2F/Application.Events%3ASensorRegisteredEvent/publish \
    -H "Content-Type: application/json" \
    -d '{
        "properties": {},
        "routing_key": "",
        "payload": "{\"sensorId\":\"test-'$(date +%s)'\",\"sensorType\":\"temperature\",\"registeredAt\":\"'$(date -Iseconds)'\"}",
        "payload_encoding": "string"
    }' | jq '.'

sleep 2

# 3. 检查队列状态（发送后）
echo -e "\n3. 发送后队列状态:"
curl -s -u guest:guest \
    http://localhost:15672/api/queues/%2F/sensor-registered-queue \
    | jq '{name, messages_ready, messages_unacknowledged, consumers, messages}'

# 4. 获取消息
echo -e "\n4. 获取消息内容:"
curl -s -u guest:guest \
    http://localhost:15672/api/queues/%2F/sensor-registered-queue/get \
    -H "Content-Type: application/json" \
    -d '{"count":1,"ackmode":"ack_requeue_false","encoding":"auto"}' \
    | jq '.[] | {payload, payload_encoding, properties}'

# 5. 再次检查队列状态（消费后）
echo -e "\n5. 消费后队列状态:"
curl -s -u guest:guest \
    http://localhost:15672/api/queues/%2F/sensor-registered-queue \
    | jq '{name, messages_ready, messages_unacknowledged, consumers, messages}'
