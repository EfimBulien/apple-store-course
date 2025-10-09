import psycopg2
import os
from dotenv import load_dotenv
import sys
import json

def call_create_order(user_id, items_json):
    load_dotenv()
    
    db_host = os.getenv('DB_HOST')
    db_name = os.getenv('DB_NAME')
    db_user = os.getenv('DB_USER')
    db_password = os.getenv('DB_PASSWORD')
    
    try:
        conn = psycopg2.connect(
            host=db_host,
            database=db_name,
            user=db_user,
            password=db_password
        )

        cur = conn.cursor()
        cur.execute("CALL sp_create_order(%s, %s::jsonb, %s)", (user_id, items_json, None))

        conn.commit()
        print("Процедура sp_create_order успешно выполнена")
        
    except (Exception, psycopg2.DatabaseError) as error:
        print("Ошибка при вызове процедуры:", error)
    finally:
        if conn is not None:
            cur.close()
            conn.close()

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Использование: python script.py <user_id> <items_json>")
        sys.exit(1)
    
    try:
        user_id = int(sys.argv[1])
        items_json = sys.argv[2]
        json.loads(items_json)
    except ValueError as e:
        print("Ошибка: user_id должен быть целым числом, а items_json - валидным JSON")
        sys.exit(1)
    
    call_create_order(user_id, items_json)