package main

import (
    "database/sql"
    "fmt"
    "os"
    "strconv"
    _ "github.com/lib/pq"
    "github.com/joho/godotenv"
)

func callCreateOrder(userID int, itemsJSON string) error {
    err := godotenv.Load()
    if err != nil {
        return fmt.Errorf("ошибка загрузки .env файла: %v", err)
    }
    
    dbHost := os.Getenv("DB_HOST")
    dbName := os.Getenv("DB_NAME")
    dbUser := os.Getenv("DB_USER")
    dbPassword := os.Getenv("DB_PASSWORD")
    
    connStr := fmt.Sprintf("host=%s dbname=%s user=%s password=%s sslmode=disable", dbHost, dbName, dbUser, dbPassword)
    
    db, err := sql.Open("postgres", connStr)
    if err != nil {
        return fmt.Errorf("ошибка подключения: %v", err)
    }
    defer db.Close()
    
    _, err = db.Exec("CALL sp_create_order($1, $2::jsonb, $3)", userID, itemsJSON, nil)
    if err != nil {
        return fmt.Errorf("ошибка при вызове процедуры: %v", err)
    }
    
    fmt.Println("Процедура sp_create_order успешно выполнена")
    return nil
}

func main() {
    if len(os.Args) != 3 {
        fmt.Println("Использование: go run script.go <user_id> <items_json>")
        os.Exit(1)
    }
    
    userID, err := strconv.Atoi(os.Args[1])
    if err != nil {
        fmt.Println("Ошибка: user_id должен быть целым числом")
        os.Exit(1)
    }
    
    itemsJSON := os.Args[2]
    if err := callCreateOrder(userID, itemsJSON); err != nil {
        fmt.Println(err)
        os.Exit(1)
    }
}