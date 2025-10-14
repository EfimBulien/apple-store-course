CREATE TABLE roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE warehouses (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL
);

INSERT INTO roles (id, name) VALUES (1, 'Admin'), (3, 'Customer') ON CONFLICT (id) DO NOTHING;
INSERT INTO warehouses (name) VALUES ('Москва'), ('Санкт-Петербург') ON CONFLICT (name) DO NOTHING;

-- 2) Пользователи (только аутентификация)
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    middle_name VARCHAR(100),
    phone VARCHAR(30),
    role_id INT NOT NULL REFERENCES roles(id) ON DELETE RESTRICT,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL
);

-- 3) Адреса 3нф
CREATE TABLE addresses (
    id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(id) ON DELETE CASCADE,
    label TEXT,
    country VARCHAR(100) NOT NULL,
    region VARCHAR(100),
    city VARCHAR(100) NOT NULL,
    street VARCHAR(200) NOT NULL,
    house VARCHAR(50) NOT NULL,
    apartment VARCHAR(50),
    postcode VARCHAR(20),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL
);

-- 4) Покупатели
CREATE TABLE customers (
    user_id INT PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    shipping_address_id INT REFERENCES addresses(id) ON DELETE SET NULL,
    billing_address_id INT REFERENCES addresses(id) ON DELETE SET NULL,
    loyalty_points INT DEFAULT 0 CHECK (loyalty_points >= 0)
);

-- 5) Категории
CREATE TABLE categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    parent_id INT REFERENCES categories(id) ON DELETE SET NULL,
    description TEXT
);

CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    sku VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(255) NOT NULL,
    category_id INT REFERENCES categories(id) ON DELETE SET NULL,
    description TEXT,
    active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL,
    avg_rating NUMERIC(3,2),
    reviews_count INT DEFAULT 0
);

CREATE TABLE product_variants (
    id SERIAL PRIMARY KEY,
    product_id INT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    variant_code VARCHAR(100) NOT NULL,
    price NUMERIC(12,2) NOT NULL CHECK (price >= 0),
    color VARCHAR(50),
    storage_gb INT CHECK (storage_gb >= 0),
    ram INT CHECK (ram >= 0),
    UNIQUE(product_id, variant_code)
);

-- ++ Добавлена таблица картинок товаров
CREATE TABLE product_images (
    id SERIAL PRIMARY KEY,
    product_variant_id INT NOT NULL REFERENCES product_variants(id) ON DELETE CASCADE,
    image_url TEXT NOT NULL, -- ссылка на изображения в minio s3
    alt_text VARCHAR(255), -- текст для seo и доступности
    sort_order INT DEFAULT 0, -- порядок отображения для картинок где 0 - начало
    is_primary BOOLEAN DEFAULT FALSE, -- флаг главного фото/нужен чисто для превьюшки
    uploaded_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL
);

-- 6) Склад и движения
CREATE TABLE inventory (
    id SERIAL PRIMARY KEY,
    product_variant_id INT NOT NULL REFERENCES product_variants(id) ON DELETE CASCADE,
    warehouse_id INT REFERENCES warehouses(id) ON DELETE CASCADE,
    --warehouse VARCHAR(100) NOT NULL,
    quantity INT NOT NULL DEFAULT 0 CHECK (quantity >= 0),
    reserve INT NOT NULL DEFAULT 0 CHECK (reserve >= 0),
    UNIQUE(product_variant_id, warehouse_id)
);

CREATE TABLE inventory_movements (
    id SERIAL PRIMARY KEY,
    product_variant_id INT NOT NULL REFERENCES product_variants(id) ON DELETE CASCADE,
    warehouse_id INT NOT NULL REFERENCES warehouses(id) ON DELETE CASCADE,
    change_qty INT NOT NULL,
    reason VARCHAR(200) NOT NULL,
    created_by INT REFERENCES users(id) ON DELETE SET NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL
);

-- 7) Заказы, позиции, платежи
CREATE TABLE orders (
    id SERIAL PRIMARY KEY,
    order_number TEXT NOT NULL UNIQUE,
    user_id INT NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    status VARCHAR(30) NOT NULL DEFAULT 'new' CHECK (status IN ('new','paid','shipped','cancelled','completed','refunded')),
    total_amount NUMERIC(12,2) NOT NULL CHECK (total_amount >= 0),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL
);

CREATE TABLE order_items (
    id SERIAL PRIMARY KEY,
    order_id INT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    product_variant_id INT NOT NULL REFERENCES product_variants(id) ON DELETE RESTRICT,
    quantity INT NOT NULL CHECK (quantity > 0),
    unit_price NUMERIC(12,2) NOT NULL CHECK (unit_price >= 0),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL
);

CREATE TABLE payments (
    id SERIAL PRIMARY KEY,
    order_id INT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    provider VARCHAR(50),
    amount NUMERIC(12,2) NOT NULL CHECK (amount >= 0),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL,
    paid_at TIMESTAMP WITH TIME ZONE,
    status VARCHAR(30) NOT NULL DEFAULT 'pending' CHECK (status IN ('pending','success','failed','refunded')),
    transaction_ref VARCHAR(255)
);

-- 8) Отзывы (без тегов!)
CREATE TABLE reviews (
    id SERIAL PRIMARY KEY,
    product_id INT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    user_id INT REFERENCES users(id) ON DELETE SET NULL,
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
    comment TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL,
    is_moderated BOOLEAN NOT NULL DEFAULT FALSE,
    moderated_by INT REFERENCES users(id),
    UNIQUE(product_id, user_id)
);

-- 9) Журнал аудита
CREATE TABLE audit_log (
    id SERIAL PRIMARY KEY,
    table_name TEXT NOT NULL,
    operation CHAR(1) NOT NULL CHECK (operation IN ('I','U','D')),
    record_id TEXT,
    changed_by INT REFERENCES users(id) ON DELETE SET NULL,
    changed_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL,
    old_row JSONB,
    new_row JSONB
);

-- 10) Настройки пользователя
CREATE TABLE user_settings (
    user_id INT PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    theme VARCHAR(20) DEFAULT 'light' CHECK (theme IN ('light','dark')),
    items_per_page INT DEFAULT 20 CHECK (items_per_page > 0 AND items_per_page <= 200),
    date_format VARCHAR(50) DEFAULT 'YYYY-MM-DD',
    number_format VARCHAR(50) DEFAULT 'en_US',
    saved_filters JSONB DEFAULT '[]'::jsonb,
    hotkeys JSONB DEFAULT '[]'::jsonb,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL
);

-- 11) Бэкапы
CREATE TABLE backups (
    id SERIAL PRIMARY KEY,
    created_by INT REFERENCES users(id) ON DELETE SET NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL,
    filename TEXT,
    command TEXT,
    note TEXT
);

-- ========== ИНДЕКСЫ ==========
--CREATE INDEX IF NOT EXISTS idx_products_name_ft ON products USING gin (to_tsvector('russian', coalesce(name,'') || ' ' || coalesce(description,'')));
CREATE INDEX IF NOT EXISTS idx_inventory_variant ON inventory(product_variant_id);
CREATE INDEX IF NOT EXISTS idx_order_user ON orders(user_id);
CREATE INDEX IF NOT EXISTS idx_reviews_product ON reviews(product_id);
-- ++ добавлен индекс для быстрого поиска
CREATE OR REPLACE FUNCTION products_search_vector_update() RETURNS trigger AS $$
BEGIN
  NEW.search_vector :=
    setweight(to_tsvector('russian', lower(coalesce(NEW.name, ''))), 'A') ||
    setweight(to_tsvector('russian', lower(coalesce(NEW.description, ''))), 'B');
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE  TRIGGER trg_update_product_search_vector
    BEFORE INSERT OR UPDATE OF name, description, sku
                     ON products
                         FOR EACH ROW
                         EXECUTE FUNCTION products_search_vector_update();

CREATE INDEX IF NOT EXISTS idx_product_images_variant ON product_images(product_variant_id);
CREATE INDEX IF NOT EXISTS idx_inventory_warehouse ON inventory(warehouse_id);
CREATE INDEX IF NOT EXISTS idx_inventory_movements_warehouse ON inventory_movements(warehouse_id);
CREATE INDEX IF NOT EXISTS  idx_products_search ON products USING GIN (search_vector);

-- ========== ТРИГГЕРЫ для АУДИТА ==========
--  ТРИГГЕР АУДИТА (обновлен для minio)
CREATE OR REPLACE FUNCTION fn_audit_trigger() RETURNS trigger AS $$
DECLARE
    v_changed_by INT;
BEGIN
    --юезопасное получение ID пользователя из контекста
    BEGIN
        v_changed_by := NULLIF(current_setting('app.current_user_id', true), '')::int;
    EXCEPTION
        WHEN invalid_text_representation THEN
            v_changed_by := NULL;
        WHEN OTHERS THEN
            v_changed_by := NULL;
    END;

    -- проверка существования пользователя
    IF v_changed_by IS NOT NULL AND NOT EXISTS(SELECT 1 FROM users WHERE id = v_changed_by) THEN
        v_changed_by := NULL;
    END IF;

    IF (TG_OP = 'DELETE') THEN
        INSERT INTO audit_log(table_name, operation, record_id, changed_by, changed_at, old_row)
        VALUES (TG_TABLE_NAME, 'D', NULL, v_changed_by, now(), row_to_json(OLD));
        RETURN OLD;
    ELSIF (TG_OP = 'UPDATE') THEN
        INSERT INTO audit_log(table_name, operation, record_id, changed_by, changed_at, old_row, new_row)
        VALUES (TG_TABLE_NAME, 'U', NULL, v_changed_by, now(), row_to_json(OLD), row_to_json(NEW));
        RETURN NEW;
    ELSIF (TG_OP = 'INSERT') THEN
        INSERT INTO audit_log(table_name, operation, record_id, changed_by, changed_at, new_row)
        VALUES (TG_TABLE_NAME, 'I', NULL, v_changed_by, now(), row_to_json(NEW));
        RETURN NEW;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

--НУЖНАЯ ШТУКА ДЛЯ ИЗБЕЖАНИЯ ОШИБОК
DO $$
DECLARE
    t TEXT;
    tables TEXT[] := ARRAY[
      'users','customers','addresses','products','product_variants','product_images', -- +++ minio
      'inventory','orders','order_items','payments','reviews','inventory_movements', 'warehouses'
    ];
BEGIN
  FOREACH t IN ARRAY tables LOOP
    EXECUTE format('DROP TRIGGER IF EXISTS audit_%s ON %s;', t, t);
    EXECUTE format('CREATE TRIGGER audit_%s AFTER INSERT OR UPDATE OR DELETE ON %s FOR EACH ROW EXECUTE FUNCTION fn_audit_trigger();', t, t);
  END LOOP;
END;
$$;

-- ========== ВАЛИДАЦИЯ ==========
-- Проверка цены
CREATE OR REPLACE FUNCTION fn_check_variant() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    IF NEW.price < 0 THEN
        RAISE EXCEPTION 'Цена не может быть отрицательной';
    END IF;
    RETURN NEW;
END;
$$;
CREATE TRIGGER trg_check_variant BEFORE INSERT OR UPDATE ON product_variants
FOR EACH ROW EXECUTE FUNCTION fn_check_variant();

-- Запрет удаления товара с заказами
CREATE OR REPLACE FUNCTION fn_prevent_product_delete() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    IF (SELECT 1 FROM order_items WHERE product_variant_id IN (SELECT id FROM product_variants WHERE product_id = OLD.id) LIMIT 1) IS NOT NULL THEN
        RAISE EXCEPTION 'Невозможно удалить товар %, так как на него есть заказы', OLD.id;
    END IF;
    RETURN OLD;
END;
$$;
CREATE TRIGGER trg_prevent_product_delete BEFORE DELETE ON products
FOR EACH ROW EXECUTE FUNCTION fn_prevent_product_delete();

--ТРИГГЕР ПРОВЕРКА ОТЗЫВА ТОЛЬКО ПОСЛЕ completed
CREATE OR REPLACE FUNCTION fn_check_review_allowed() RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE
    v_order_exists BOOLEAN;
BEGIN
    SELECT EXISTS(
        SELECT 1
        FROM order_items oi
        JOIN orders o ON o.id = oi.order_id
        WHERE oi.product_variant_id IN (
            SELECT id FROM product_variants WHERE product_id = NEW.product_id
        )
        AND o.user_id = NEW.user_id
        AND o.status = 'completed'
    ) INTO v_order_exists;

    IF NOT v_order_exists THEN
        RAISE EXCEPTION 'Отзыв можно оставить только после оформления заказа на этот товар';
    END IF;

    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_check_review_allowed ON reviews;
CREATE TRIGGER trg_check_review_allowed BEFORE INSERT ON reviews
FOR EACH ROW EXECUTE FUNCTION fn_check_review_allowed();

-- ========== VIEWS ПРЕДСТАВЛЕНИЯ ==========

CREATE OR REPLACE VIEW vw_orders_summary AS
SELECT
    o.id AS order_id,
    o.order_number,
    o.user_id,
    u.first_name || ' ' || u.last_name AS user_name,
    o.status,
    o.total_amount,
    o.created_at,
    COUNT(oi.id) AS items_count
FROM orders o
LEFT JOIN users u ON u.id = o.user_id
LEFT JOIN order_items oi ON oi.order_id = o.id
GROUP BY o.id, u.first_name, u.last_name, o.order_number, o.status, o.total_amount, o.created_at;

CREATE OR REPLACE VIEW vw_product_stock AS
SELECT
    pv.id AS variant_id,
    p.name AS product_name,
    pv.variant_code,
    w.name AS warehouse,
    COALESCE(i.quantity - i.reserve, 0) AS available_qty
FROM product_variants pv
JOIN products p ON p.id = pv.product_id
JOIN inventory i ON i.product_variant_id = pv.id
JOIN warehouses w ON w.id = i.warehouse_id;

-- ========== Хранимые процедуры ==========

-- 1) sp_restock - пополнение склада
CREATE OR REPLACE PROCEDURE sp_restock(p_user_id INT, p_items JSONB)
LANGUAGE plpgsql
AS $$
DECLARE
    r RECORD;
    v_warehouse_id INT;
BEGIN
    PERFORM set_config('app.current_user_id', p_user_id::text, true);

    FOR r IN SELECT * FROM jsonb_to_recordset(p_items) AS (variant_id int, warehouse_name text, qty int) LOOP
        SELECT id INTO v_warehouse_id FROM warehouses WHERE name = r.warehouse_name;
        IF v_warehouse_id IS NULL THEN
            RAISE EXCEPTION 'Склад "%" не найден', r.warehouse_name;
        END IF;

        INSERT INTO inventory (product_variant_id, warehouse_id, quantity)
        VALUES (r.variant_id, v_warehouse_id, GREATEST(r.qty,0))
        ON CONFLICT (product_variant_id, warehouse_id)
        DO UPDATE SET quantity = inventory.quantity + GREATEST(EXCLUDED.quantity,0);

        INSERT INTO inventory_movements (product_variant_id, warehouse_id, change_qty, reason, created_by)
        VALUES (r.variant_id, v_warehouse_id, r.qty, 'restock', p_user_id);
    END LOOP;
END;
$$;

-- 2)sp_create_order - Для создагия заказа т загесегя хаказа в рехерв
CREATE OR REPLACE PROCEDURE sp_create_order(p_user_id INT, p_items JSONB, OUT p_order_id INT)
LANGUAGE plpgsql
AS $$
DECLARE
    v_item RECORD;
    v_total NUMERIC(12,2) := 0;
    v_variant_id INT;
    v_qty INT;
    v_price NUMERIC(12,2);
    v_order_number TEXT;
    v_warehouse_id INT;
    v_available INT;
BEGIN
    PERFORM set_config('app.current_user_id', p_user_id::text, true);

    IF p_items IS NULL OR jsonb_array_length(p_items) = 0 THEN
        RAISE EXCEPTION 'Корзина пуста';
    END IF;

    -- прверка доступности и подсчёт суммы
    FOR v_item IN SELECT * FROM jsonb_to_recordset(p_items) AS (variant_id int, quantity int) LOOP
        v_variant_id := v_item.variant_id;
        v_qty := v_item.quantity;

        SELECT price INTO v_price FROM product_variants WHERE id = v_variant_id;
        IF v_price IS NULL THEN
            RAISE EXCEPTION 'Такой товар % не найден', v_variant_id;
        END IF;

		-- проверка доступного колчиества с учтеом резерва
        SELECT SUM(quantity - reserve), MIN(id)
        INTO v_available, v_warehouse_id
        FROM inventory
        WHERE product_variant_id = v_variant_id
        GROUP BY product_variant_id
        HAVING SUM(quantity - reserve) >= v_qty;

        IF v_available IS NULL THEN
            RAISE EXCEPTION 'Недостаточно количества товара на складе %', v_variant_id;
        END IF;

        v_total := v_total + v_price * v_qty;
    END LOOP;

    v_order_number := 'ORD' || to_char(now(),'YYYYMMDDHH24MISS') || lpad((floor(random()*9999))::int::text,4,'0');

    INSERT INTO orders (order_number, user_id, status, total_amount)
    VALUES (v_order_number, p_user_id, 'new', v_total)
    RETURNING id INTO p_order_id;

    -- создание позиций + резервирование товара
    FOR v_item IN SELECT * FROM jsonb_to_recordset(p_items) AS (variant_id int, quantity int) LOOP
        v_variant_id := v_item.variant_id;
        v_qty := v_item.quantity;
        SELECT price INTO v_price FROM product_variants WHERE id = v_variant_id;

        INSERT INTO order_items (order_id, product_variant_id, quantity, unit_price)
        VALUES (p_order_id, v_variant_id, v_qty, v_price);

        -- ЗАРЕЗЕРВИРУЕМ ТОВАР НА СКЛАДАХЪ
        WHILE v_qty > 0 LOOP
            UPDATE inventory
            SET reserve = reserve + LEAST(v_qty, quantity - reserve)
            WHERE id = (
                SELECT id FROM inventory
                WHERE product_variant_id = v_variant_id
                  AND (quantity - reserve) > 0
                ORDER BY quantity DESC
                LIMIT 1
                FOR UPDATE SKIP LOCKED
            )
            RETURNING (quantity - reserve) INTO v_available;

            IF NOT FOUND THEN
                RAISE EXCEPTION 'Конфликт одновременных запасов для варианта %', v_variant_id;
            END IF;

            v_qty := v_qty - LEAST(v_qty, v_available);
        END LOOP;

        -- логирование резервирования в запись в таблице
        INSERT INTO inventory_movements (product_variant_id, warehouse_id, change_qty, reason, created_by)
        SELECT
            v_variant_id,
            warehouse_id,
            -v_item.quantity,  -- отрицательное изменение = в резерв
            'зарезервировано для заказа #' || p_order_id,
            p_user_id
        FROM inventory
        WHERE product_variant_id = v_variant_id
          AND reserve > 0
        LIMIT 1;
    END LOOP;

    -- Платёж
    INSERT INTO payments (order_id, provider, amount, status)
    VALUES (p_order_id, 'placeholder', v_total, 'pending');
END;
$$;

-- 3)sp_cancel_order — функция для очистки.освобождения резерва резерва
CREATE OR REPLACE PROCEDURE sp_cancel_order(p_user_id INT, p_order_id INT)
LANGUAGE plpgsql
AS $$
DECLARE
    r RECORD;
BEGIN
    PERFORM set_config('app.current_user_id', p_user_id::text, true);

    IF NOT EXISTS (SELECT 1 FROM orders WHERE id = p_order_id) THEN
        RAISE EXCEPTION 'Заказ % не найден', p_order_id;
    END IF;

    UPDATE orders SET status = 'cancelled', updated_at = now() WHERE id = p_order_id;

    -- освобождение зарезервированного товара
    FOR r IN SELECT product_variant_id, quantity FROM order_items WHERE order_id = p_order_id LOOP
        UPDATE inventory
        SET reserve = reserve - LEAST(reserve, r.quantity)
        -- было исправлено с product_variant_id = r.product_id
        WHERE product_variant_id = r.product_variant_id
        AND reserve > 0;

        INSERT INTO inventory_movements (product_variant_id, warehouse_id, change_qty, reason, created_by)
        SELECT
            r.product_variant_id,
            warehouse_id,
            r.quantity,
            'резерв освобождается после отмены #' || p_order_id,
            p_user_id
        FROM inventory
        WHERE product_variant_id = r.product_variant_id
          AND reserve >= 0
        LIMIT 1;
    END LOOP;

    UPDATE payments SET status = 'refunded' WHERE order_id = p_order_id;
END;
$$;

-- 4)sp_complete_order
CREATE OR REPLACE PROCEDURE sp_complete_order(p_user_id INT, p_order_id INT)
LANGUAGE plpgsql
AS $$
DECLARE
    r RECORD;
BEGIN
    PERFORM set_config('app.current_user_id', p_user_id::text, true);

    IF NOT EXISTS (SELECT 1 FROM orders WHERE id = p_order_id AND status = 'shipped') THEN
        RAISE EXCEPTION 'Для завершения заказа он должен иметь статус "shipped"';
    END IF;

    --Списание зарезервированного товар
    FOR r IN SELECT product_variant_id, quantity FROM order_items WHERE order_id = p_order_id LOOP
        UPDATE inventory
        SET
            quantity = quantity - LEAST(quantity, r.quantity),
            reserve = reserve - LEAST(reserve, r.quantity)
        WHERE product_variant_id = r.product_variant_id
        AND reserve >= r.quantity;

        INSERT INTO inventory_movements (product_variant_id, warehouse_id, change_qty, reason, created_by)
        SELECT
            r.product_variant_id,
            warehouse_id,
            -r.quantity,
            'вычитается после завершения заказа #' || p_order_id,
            p_user_id
        FROM inventory
        WHERE product_variant_id = r.product_variant_id
        LIMIT 1;
    END LOOP;

    UPDATE orders SET status = 'completed', updated_at = now() WHERE id = p_order_id;
END;
$$;

-- 5) recalc_product_rating
CREATE OR REPLACE FUNCTION recalc_product_rating(p_product_id INT) RETURNS VOID LANGUAGE plpgsql AS $$
DECLARE
    v_avg NUMERIC(3,2);
    v_cnt INT;
BEGIN
    SELECT AVG(rating)::numeric(3,2), COUNT(*) INTO v_avg, v_cnt FROM reviews WHERE product_id = p_product_id AND is_moderated = FALSE;
    UPDATE products SET avg_rating = v_avg, reviews_count = v_cnt WHERE id = p_product_id;
END;
$$;

-- триггер на отзывы
CREATE OR REPLACE FUNCTION trg_reviews_after() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
  PERFORM recalc_product_rating(COALESCE(NEW.product_id, OLD.product_id));
  RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_reviews_after ON reviews;
CREATE TRIGGER trg_reviews_after AFTER INSERT OR UPDATE OR DELETE ON reviews FOR EACH ROW EXECUTE FUNCTION trg_reviews_after();

-- ========== для BACKUP/RESTORE ==========
CREATE OR REPLACE FUNCTION sp_request_backup(p_user_id INT, p_note TEXT DEFAULT NULL)
RETURNS TABLE(backup_id INT, command TEXT) LANGUAGE plpgsql AS $$
DECLARE
    v_filename TEXT := 'shop_backup_' || to_char(now(),'YYYYMMDD_HH24MISS') || '.dump';
    v_cmd TEXT;
    v_id INT;
BEGIN
    v_cmd := format('pg_dump -Fc -f %s -d %%DBNAME%% --no-owner --schema=public', v_filename);
    INSERT INTO backups (created_by, filename, command, note) VALUES (p_user_id, v_filename, v_cmd, p_note) RETURNING id INTO v_id;
    RETURN QUERY SELECT v_id, v_cmd;
END;
$$;

-- ========== ЭКСПОРТ ==========
CREATE OR REPLACE FUNCTION export_audit(p_from TIMESTAMPTZ, p_to TIMESTAMPTZ) RETURNS JSONB LANGUAGE sql AS $$
  SELECT coalesce(jsonb_agg(row_to_json(a)), '[]'::jsonb) FROM (SELECT * FROM audit_log WHERE changed_at BETWEEN p_from AND p_to ORDER BY changed_at) a;
$$;

-- ========== РАНДоМНЫЕ ТЕСТОВЫЕ ДАННЫЕ ==========
-- заполнение пользователей
INSERT INTO users (username, email, password_hash, first_name, last_name, middle_name, phone, role_id)
VALUES
('admin', 'admin@example.com', 'HASH_PLACEHOLDER', 'Андрей', 'Иванов', 'Петрович', '+79161234567', 1)
ON CONFLICT (username) DO NOTHING;

INSERT INTO users (username, email, password_hash, first_name, last_name, middle_name, phone, role_id)
VALUES
('customer1', 'cust1@example.com', 'HASH_PLACEHOLDER', 'Иван', 'Петров', 'Сергеевич', '+79161234568', 3)
ON CONFLICT (username) DO NOTHING;

--адрес покупателя
INSERT INTO addresses (user_id, label, country, region, city, street, house, apartment, postcode)
SELECT u.id, 'Дом', 'Россия', 'Московская область', 'Москва', 'Ленинский проспект', '42', '15', '119334'
FROM users u WHERE u.username = 'customer1'
ON CONFLICT DO NOTHING;

-- Покупатель
INSERT INTO customers (user_id, shipping_address_id, billing_address_id, loyalty_points)
SELECT u.id, a.id, a.id, 100
FROM users u
JOIN addresses a ON a.user_id = u.id
WHERE u.username = 'customer1'
ON CONFLICT (user_id) DO NOTHING;
-- Категории и товары
INSERT INTO categories (name) VALUES ('iPhone') ON CONFLICT (name) DO NOTHING;
INSERT INTO categories (name) VALUES ('MacBook') ON CONFLICT (name) DO NOTHING;

INSERT INTO products (sku, name, category_id, description)
VALUES
('IP16', 'iPhone 16', (SELECT id FROM categories WHERE name='iPhone'), 'Самый последний крутой Айфон')
ON CONFLICT (sku) DO NOTHING;

INSERT INTO products (sku, name, category_id, description)
VALUES
('MBP16', 'MacBook Pro 16 M3 Pro', (SELECT id FROM categories WHERE name='MacBook'), 'Мощный ноутбук для профессиональной работы')
ON CONFLICT (sku) DO NOTHING;

INSERT INTO product_variants (product_id, variant_code, price, color, storage_gb)
VALUES
( (SELECT id FROM products WHERE sku='IP16'), 'IPH16-BLK-128', 79999.99, 'Чёрный', 128 )
ON CONFLICT (product_id, variant_code) DO NOTHING;

INSERT INTO product_variants (product_id, variant_code, price, color, storage_gb)
VALUES
( (SELECT id FROM products WHERE sku='IP16'), 'IPH16-WHT-256', 89999.99, 'Белый', 256 )
ON CONFLICT (product_id, variant_code) DO NOTHING;

INSERT INTO product_variants (product_id, variant_code, price, color, storage_gb)
VALUES
( (SELECT id FROM products WHERE sku='MBP16'), 'MBP16-SIL-512', 199999.00, 'Серебристый', 512 )
ON CONFLICT (product_id, variant_code) DO NOTHING;

--настрокйки пользователкей
INSERT INTO user_settings (user_id, theme, items_per_page, date_format, number_format)
SELECT id, 'light', 20, 'DD.MM.YYYY', 'ru_RU'
FROM users WHERE username = 'customer1'
ON CONFLICT (user_id) DO NOTHING;

INSERT INTO product_images (product_variant_id, image_url, alt_text, sort_order, is_primary)
VALUES
(
    1,
    'https://minio.com/images/iphone-16-black-128gb-1.jpg', -- условная ссылка на bucket
    'iPhone 16 Black 128GB - вид спереди',
    0,
    TRUE
),
(
    1,
    'https://minio.com/images/iphone-16-black-128gb-2.jpg', -- условная ссылка на bucket
    'iPhone 16 Black 128GB - вид сзади',
    1,
    FALSE
)
ON CONFLICT DO NOTHING;

-- НАПОЛНЯЕМ СКЛАД
CALL sp_restock(1, '[{"variant_id":1,"warehouse_name":"Москва","qty":25}, {"variant_id":2,"warehouse_name":"Москва","qty":15}, {"variant_id":3,"warehouse_name":"Санкт-Петербург","qty":8}]'::jsonb);

--проверка наличия через представление
SELECT * FROM vw_product_stock ORDER BY variant_id;

-- создание заказа
CALL sp_create_order(2, '[{"variant_id": 1, "quantity": 1}]'::jsonb, NULL);

-- завершение заказа (чтобы можно было оставить отзыв)
DO $$
DECLARE
    v_order_id INT;
BEGIN
    SELECT id INTO v_order_id FROM orders WHERE user_id = 2 ORDER BY id DESC LIMIT 1;
    IF v_order_id IS NOT NULL THEN
        UPDATE orders SET status = 'shipped' WHERE id = v_order_id;
        CALL sp_complete_order(2, v_order_id);
    END IF;
END $$;

--оставляем отзыв
INSERT INTO reviews (product_id, user_id, rating, comment)
VALUES (
    (SELECT id FROM products WHERE sku='IP16'),
    (SELECT id FROM users WHERE username='customer1'),
    5,
    'Отличный телефон, рекомендую!'
)
ON CONFLICT DO NOTHING;

--проверка рейтинга
SELECT name, avg_rating, reviews_count FROM products WHERE sku = 'IP16';

-- ========== ТЕСТ: НОВЫЙ ПОКУПАТЕЛЬ ==========

DO $$
DECLARE
    v_new_user_id INT;
    v_new_address_id INT;
    v_order_id INT;
    v_product_id INT;
BEGIN
    PERFORM set_config('app.current_user_id', (SELECT id::text FROM users WHERE username = 'admin'), true);

    INSERT INTO users (username, email, password_hash, first_name, last_name, middle_name, phone, role_id)
    VALUES ('customer_ru', 'customer_ru@example.com', 'HASH_PLACEHOLDER', 'Мария', 'Сидорова', 'Ивановна', '+79161234569', 3)
    ON CONFLICT (username) DO NOTHING
    RETURNING id INTO v_new_user_id;

    IF v_new_user_id IS NULL THEN
        SELECT id INTO v_new_user_id FROM users WHERE username = 'customer_ru';
    END IF;

    PERFORM set_config('app.current_user_id', v_new_user_id::text, true);

    -- Санкт-Петербург
    INSERT INTO addresses (user_id, label, country, region, city, street, house, apartment, postcode)
    VALUES (v_new_user_id, 'Квартира', 'Россия', 'Ленинградская область', 'Санкт-Петербург', 'Невский проспект', '25', '7', '191186')
    RETURNING id INTO v_new_address_id;

    -- новые настройки
    INSERT INTO user_settings (user_id, theme, items_per_page, date_format, number_format)
    VALUES (v_new_user_id, 'dark', 30, 'DD.MM.YYYY', 'ru_RU')
    ON CONFLICT (user_id) DO NOTHING;

    INSERT INTO customers (user_id, shipping_address_id, billing_address_id, loyalty_points)
    VALUES (v_new_user_id, v_new_address_id, v_new_address_id, 50)
    ON CONFLICT (user_id) DO NOTHING;

    SELECT id INTO v_product_id FROM products WHERE sku = 'MBP16';

    CALL sp_create_order(v_new_user_id, '[{"variant_id": 3, "quantity": 1}]'::jsonb, v_order_id);


    -- оставить отзыв без этого не получится
    UPDATE orders SET status = 'shipped' WHERE id = v_order_id;
    CALL sp_complete_order(v_new_user_id, v_order_id);
    
    -- правда оплата так и остается пустой и ожидает оплаты

    INSERT INTO reviews (product_id, user_id, rating, comment)
    VALUES (v_product_id, v_new_user_id, 5, 'Прекрасный ноутбук! Быстрый, экран отличный. Идеален для работы и творчества.')
    ON CONFLICT DO NOTHING;
END $$;

-- Финальная проверка
SELECT u.username, r.rating, r.comment, o.status as last_order_status
FROM reviews r
JOIN users u ON u.id = r.user_id
JOIN order_items oi ON oi.product_variant_id IN (SELECT id FROM product_variants WHERE product_id = r.product_id)
JOIN orders o ON o.id = oi.order_id AND o.user_id = r.user_id
WHERE r.product_id = (SELECT id FROM products WHERE sku = 'IP16')
ORDER BY r.created_at DESC;

-- доп проверка
SELECT
    u.first_name,
    u.last_name,
    a.country,
    a.city,
    a.street,
    a.house,
    a.apartment
FROM addresses a
JOIN users u ON u.id = a.user_id
WHERE a.country = 'Россия';

