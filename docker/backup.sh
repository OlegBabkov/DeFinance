#!/bin/sh
TIMESTAMP=$(date +"%Y-%m-%d_%H-%M-%S")
FILE="/backups/definance_${TIMESTAMP}.sql.gz"

echo "[$(date)] Starting backup → $FILE"

pg_dump -h postgres -U definance -d definance | gzip > "$FILE"

if [ $? -eq 0 ]; then
  echo "[$(date)] Backup successful: $FILE"
else
  echo "[$(date)] Backup FAILED"
  rm -f "$FILE"
  exit 1
fi

# Retention: remove backups older than 30 days
find /backups -name "definance_*.sql.gz" -mtime +30 -delete
echo "[$(date)] Cleanup done"
