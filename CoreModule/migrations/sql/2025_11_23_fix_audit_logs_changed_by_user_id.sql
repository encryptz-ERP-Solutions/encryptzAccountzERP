-- ================================================================================================
-- Script: 2025_11_23_fix_audit_logs_changed_by_user_id.sql
-- Purpose: Create audit_logs table if it doesn't exist, or add changed_by_user_id column if missing
-- ================================================================================================

-- First, check if table exists and what columns it has
DO $$
BEGIN
    -- Create table only if it doesn't exist
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'core' AND table_name = 'audit_logs'
    ) THEN
        CREATE TABLE core.audit_logs (
            audit_log_id BIGSERIAL PRIMARY KEY,
            table_name VARCHAR(100) NOT NULL,
            record_id VARCHAR(255) NOT NULL,
            action VARCHAR(20) NOT NULL CHECK (action IN ('INSERT', 'UPDATE', 'DELETE')),
            changed_by_user_id UUID NULL,
            changed_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
            old_values JSONB NULL,
            new_values JSONB NULL,
            ip_address INET NULL,
            user_agent TEXT NULL
        );
        RAISE NOTICE 'Created audit_logs table';
    ELSE
        RAISE NOTICE 'Table audit_logs already exists, checking columns...';
        
        -- Add missing columns one by one (skip audit_log_id if table already has a primary key)
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'core' AND table_name = 'audit_logs' AND column_name = 'audit_log_id'
        ) THEN
            -- Check if table already has a primary key
            IF NOT EXISTS (
                SELECT 1 FROM information_schema.table_constraints 
                WHERE table_schema = 'core' AND table_name = 'audit_logs' AND constraint_type = 'PRIMARY KEY'
            ) THEN
                ALTER TABLE core.audit_logs ADD COLUMN audit_log_id BIGSERIAL PRIMARY KEY;
                RAISE NOTICE 'Added audit_log_id column as PRIMARY KEY';
            ELSE
                ALTER TABLE core.audit_logs ADD COLUMN audit_log_id BIGSERIAL;
                RAISE NOTICE 'Added audit_log_id column (table already has primary key)';
            END IF;
        END IF;
        
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'core' AND table_name = 'audit_logs' AND column_name = 'table_name'
        ) THEN
            ALTER TABLE core.audit_logs ADD COLUMN table_name VARCHAR(100);
            RAISE NOTICE 'Added table_name column';
        END IF;
        
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'core' AND table_name = 'audit_logs' AND column_name = 'record_id'
        ) THEN
            ALTER TABLE core.audit_logs ADD COLUMN record_id VARCHAR(255);
            RAISE NOTICE 'Added record_id column';
        END IF;
        
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'core' AND table_name = 'audit_logs' AND column_name = 'action'
        ) THEN
            ALTER TABLE core.audit_logs ADD COLUMN action VARCHAR(20);
            RAISE NOTICE 'Added action column';
        END IF;
        
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'core' AND table_name = 'audit_logs' AND column_name = 'changed_by_user_id'
        ) THEN
            ALTER TABLE core.audit_logs ADD COLUMN changed_by_user_id UUID NULL;
            RAISE NOTICE 'Added changed_by_user_id column';
        END IF;
        
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'core' AND table_name = 'audit_logs' AND column_name = 'changed_at_utc'
        ) THEN
            ALTER TABLE core.audit_logs ADD COLUMN changed_at_utc TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC');
            RAISE NOTICE 'Added changed_at_utc column';
        END IF;
        
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'core' AND table_name = 'audit_logs' AND column_name = 'old_values'
        ) THEN
            ALTER TABLE core.audit_logs ADD COLUMN old_values JSONB NULL;
            RAISE NOTICE 'Added old_values column';
        END IF;
        
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'core' AND table_name = 'audit_logs' AND column_name = 'new_values'
        ) THEN
            ALTER TABLE core.audit_logs ADD COLUMN new_values JSONB NULL;
            RAISE NOTICE 'Added new_values column';
        END IF;
        
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'core' AND table_name = 'audit_logs' AND column_name = 'ip_address'
        ) THEN
            ALTER TABLE core.audit_logs ADD COLUMN ip_address INET NULL;
            RAISE NOTICE 'Added ip_address column';
        END IF;
        
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'core' AND table_name = 'audit_logs' AND column_name = 'user_agent'
        ) THEN
            ALTER TABLE core.audit_logs ADD COLUMN user_agent TEXT NULL;
            RAISE NOTICE 'Added user_agent column';
        END IF;
    END IF;
END $$;

-- Create indexes for common queries (only if columns exist)
DO $$
BEGIN
    -- Check if table_name and record_id exist before creating index
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' AND table_name = 'audit_logs' AND column_name = 'table_name'
    ) AND EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' AND table_name = 'audit_logs' AND column_name = 'record_id'
    ) THEN
        CREATE INDEX IF NOT EXISTS idx_audit_logs_table_record 
            ON core.audit_logs(table_name, record_id);
    END IF;
    
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' AND table_name = 'audit_logs' AND column_name = 'changed_by_user_id'
    ) THEN
        CREATE INDEX IF NOT EXISTS idx_audit_logs_changed_by 
            ON core.audit_logs(changed_by_user_id);
    END IF;
    
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' AND table_name = 'audit_logs' AND column_name = 'changed_at_utc'
    ) THEN
        CREATE INDEX IF NOT EXISTS idx_audit_logs_changed_at 
            ON core.audit_logs(changed_at_utc DESC);
    END IF;
    
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' AND table_name = 'audit_logs' AND column_name = 'action'
    ) THEN
        CREATE INDEX IF NOT EXISTS idx_audit_logs_action 
            ON core.audit_logs(action);
    END IF;
END $$;

-- Add foreign key constraint if users table exists and constraint doesn't exist
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'core' AND table_name = 'users') THEN
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.table_constraints 
            WHERE table_schema = 'core' 
            AND table_name = 'audit_logs' 
            AND constraint_name = 'fk_audit_logs_changed_by_user_id'
        ) THEN
            ALTER TABLE core.audit_logs 
            ADD CONSTRAINT fk_audit_logs_changed_by_user_id 
            FOREIGN KEY (changed_by_user_id) REFERENCES core.users(user_id) ON DELETE SET NULL;
            
            RAISE NOTICE 'Added foreign key constraint fk_audit_logs_changed_by_user_id';
        END IF;
    END IF;
END $$;

-- If table already existed, check if changed_by_user_id column exists and add it if missing
DO $$
BEGIN
    -- Check if changed_by_user_id column exists
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_schema = 'core' 
        AND table_name = 'audit_logs' 
        AND column_name = 'changed_by_user_id'
    ) THEN
        -- Add the column
        ALTER TABLE core.audit_logs 
        ADD COLUMN changed_by_user_id UUID NULL;
        
        -- Create index if it doesn't exist
        CREATE INDEX IF NOT EXISTS idx_audit_logs_changed_by 
        ON core.audit_logs(changed_by_user_id);
        
        -- Add foreign key constraint if users table exists
        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'core' AND table_name = 'users') THEN
            IF NOT EXISTS (
                SELECT 1 FROM information_schema.table_constraints 
                WHERE table_schema = 'core' 
                AND table_name = 'audit_logs' 
                AND constraint_name = 'fk_audit_logs_changed_by_user_id'
            ) THEN
                ALTER TABLE core.audit_logs 
                ADD CONSTRAINT fk_audit_logs_changed_by_user_id 
                FOREIGN KEY (changed_by_user_id) REFERENCES core.users(user_id) ON DELETE SET NULL;
            END IF;
        END IF;
        
        RAISE NOTICE 'Added changed_by_user_id column to audit_logs table';
    ELSE
        RAISE NOTICE 'Column changed_by_user_id already exists in audit_logs table';
    END IF;
END $$;

-- Verification - Show all columns in audit_logs table
SELECT 
    column_name, 
    data_type, 
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_schema = 'core' 
AND table_name = 'audit_logs' 
ORDER BY ordinal_position;

