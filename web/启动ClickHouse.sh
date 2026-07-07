docker run -d \
  --name clickhouse \
  -p 18123:8123 \
  -p 19000:9000 \
  -e CLICKHOUSE_USER=admin \
  -e CLICKHOUSE_PASSWORD=123456 \
  -v /data/clickhouse/data:/var/lib/clickhouse \
  -v /data/clickhouse/logs:/var/log/clickhouse-server \
  --ulimit nofile=262144:262144 \
  clickhouse/clickhouse-server:26.3.9.8