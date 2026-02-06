#!/bin/bash

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
sleep 30

# Run the initialization script
echo "Running database initialization script..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P DataDic123! -C -i /scripts/01-init-db.sql

if [ $? -eq 0 ]; then
    echo "Database initialized successfully!"
else
    echo "Database initialization failed!"
    exit 1
fi
