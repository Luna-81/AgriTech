#!/bin/bash
echo "========================================="
echo "   Day 6 验收测试脚本"
echo "========================================="
GREEN='\033[0;32m'; RED='\033[0;31m'; YELLOW='\033[1;33m'; NC='\033[0m'
echo -e "\n${YELLOW}1. 检查服务状态...${NC}"
docker ps | grep -q "agritech_postgres" && echo -e "${GREEN}✅ PostgreSQL 运行中${NC}" || echo -e "${RED}❌ PostgreSQL 未运行${NC}"
docker ps | grep -q "agritech_rabbitmq" && echo -e "${GREEN}✅ RabbitMQ 运行中${NC}" || echo -e "${RED}❌ RabbitMQ 未运行${NC}"
echo -e "\n${YELLOW}2. 检查应用健康...${NC}"
curl -s http://192.168.1.224:5000/health > /dev/null && echo -e "${GREEN}✅ 应用运行中${NC}" || echo -e "${RED}❌ 应用未运行，请启动: cd src/WebAPI && dotnet run${NC}"
echo -e "\n${YELLOW}3. 检查队列状态...${NC}"
curl -s -u guest:guest http://192.168.1.224:15672/api/queues/%2F/sensor-registered-queue | jq '{name, messages_ready, messages_unacknowledged, consumers, messages}' 2>/dev/null
echo -e "\n${YELLOW}4. 发送测试消息...${NC}"
FARM_ID="50a8db2d-8502-44aa-aa4e-05e740d7444a"
RESPONSE=$(curl -s -X POST http://192.168.1.224:5000/api/v1/sensors -H "Content-Type: application/json" -d "{\"name\":\"测试_$(date +%s)\",\"temperatureThreshold\":30,\"latitude\":31.2304,\"longitude\":121.4737,\"farmId\":\"$FARM_ID\"}")
echo "$RESPONSE" | jq -e '.success' > /dev/null 2>&1 && echo -e "${GREEN}✅ 消息发送成功！SensorId: $(echo "$RESPONSE" | jq -r '.data')${NC}" || echo -e "${RED}❌ 消息发送失败${NC}"
echo -e "\n${YELLOW}5. 发送后队列状态...${NC}"
sleep 2
curl -s -u guest:guest http://192.168.1.224:15672/api/queues/%2F/sensor-registered-queue | jq '{name, messages_ready, messages_unacknowledged, messages}' 2>/dev/null
echo -e "\n${YELLOW}6. 查看死信队列...${NC}"
DLQ=$(curl -s -u guest:guest http://192.168.1.224:15672/api/queues/%2F/sensor-registered-queue_error)
DLQ_COUNT=$(echo "$DLQ" | jq -r '.messages' 2>/dev/null)
[ "$DLQ_COUNT" = "null" ] && echo -e "${GREEN}✅ 死信队列为空${NC}" || echo -e "${YELLOW}⚠️  死信队列有 $DLQ_COUNT 条消息${NC}"
echo -e "\n========================================="
echo -e "${GREEN}  测试完成！${NC}"
echo -e "========================================="
