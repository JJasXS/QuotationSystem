using FirebirdSql.Data.FirebirdClient;
using Microsoft.Extensions.Configuration;

namespace QuotationSystem.Services
{
    public class DbInitializer
    {
      private readonly DbHelper _db;

      public DbInitializer(DbHelper db)
      {
        _db = db;
      }

        public void Initialize()
        {
            var tables = new[]
            {
                "CHAT_SESSION",
                "CHAT_MESSAGE",
                "QUOTATION_REQUEST",
                "QUOTATION_REQUEST_ITEM"
            };
            using var conn = _db.GetConnection();
            using var cmd = conn.CreateCommand();
            foreach (var table in tables)
            {
                cmd.CommandText = $"SELECT 1 FROM RDB$RELATIONS WHERE RDB$RELATION_NAME = '{table}'";
                var exists = false;
                using (var reader = cmd.ExecuteReader())
                {
                    exists = reader.Read();
                }
                if (exists)
                {
                    Console.WriteLine($"[DB INIT] Table '{table}' exists.");
                }
                else
                {
                    Console.WriteLine($"[DB INIT] Table '{table}' does NOT exist. Attempting to create...");
                }
            }
            // Create sequences if missing
            var sequences = new[] {
                "SEQ_CHAT_MESSAGE",
                "SEQ_QUOTATION_REQUEST",
                "SEQ_QUOTATION_REQUEST_ITEM"
            };
            foreach (var seq in sequences)
            {
                cmd.CommandText = $"SELECT 1 FROM RDB$GENERATORS WHERE RDB$GENERATOR_NAME = '{seq}'";
                var exists = false;
                using (var reader = cmd.ExecuteReader())
                {
                    exists = reader.Read();
                }
                if (!exists)
                {
                    cmd.CommandText = $"CREATE SEQUENCE {seq}";
                    try { cmd.ExecuteNonQuery(); Console.WriteLine($"[DB INIT] Sequence '{seq}' created."); } catch { Console.WriteLine($"[DB INIT] Sequence '{seq}' creation failed or already exists."); }
                }
                else
                {
                    Console.WriteLine($"[DB INIT] Sequence '{seq}' exists.");
                }
            }

            // Table creation validation
            var tableDefinitions = new[] {
                new { Name = "CHAT_SESSION", Sql = @"CREATE TABLE CHAT_SESSION (
    SESSION_ID VARCHAR(50) PRIMARY KEY,
    CREATED_AT TIMESTAMP,
    LAST_ACTIVITY_AT TIMESTAMP
)" },
                new { Name = "CHAT_MESSAGE", Sql = @"CREATE TABLE CHAT_MESSAGE (
    MSG_ID BIGINT PRIMARY KEY,
    SESSION_ID VARCHAR(50),
    SENDER VARCHAR(10),
    MESSAGE_TEXT VARCHAR(4000),
    CREATED_AT TIMESTAMP,
    SENDER_TYPE VARCHAR(10),
    SENDER_CUSTOMER_CODE VARCHAR(40),
    SENDER_ADMIN_CODE VARCHAR(40),
    SENDER_NAME_SNAPSHOT VARCHAR(200),
    FOREIGN KEY (SESSION_ID) REFERENCES CHAT_SESSION(SESSION_ID)
)" },
                new { Name = "QUOTATION_REQUEST", Sql = @"CREATE TABLE QUOTATION_REQUEST (
    REQUEST_ID BIGINT PRIMARY KEY,
    SESSION_ID VARCHAR(50),
    SEARCH_MODE VARCHAR(20),
    STOCKGROUP VARCHAR(100),
    KEYWORDS_CSV VARCHAR(500),
    STATUS VARCHAR(20),
    CREATED_AT TIMESTAMP,
    APPROVED_AT TIMESTAMP,
    ADMIN_NOTE VARCHAR(2000)
)" },
                new { Name = "QUOTATION_REQUEST_ITEM", Sql = @"CREATE TABLE QUOTATION_REQUEST_ITEM (
    REQUEST_ITEM_ID BIGINT PRIMARY KEY,
    REQUEST_ID BIGINT,
    ITEM_CODE VARCHAR(80),
    DESCRIPTION_SNAPSHOT VARCHAR(800),
    UOM_SNAPSHOT VARCHAR(40),
    UNIT_PRICE_SNAPSHOT NUMERIC(18,2),
    QTY NUMERIC(18,2),
    LINE_TOTAL NUMERIC(18,2),
    FOREIGN KEY (REQUEST_ID) REFERENCES QUOTATION_REQUEST(REQUEST_ID)
)" }
            };
            foreach (var table in tableDefinitions)
            {
                cmd.CommandText = $"SELECT 1 FROM RDB$RELATIONS WHERE RDB$RELATION_NAME = '{table.Name}'";
                var exists = false;
                using (var reader = cmd.ExecuteReader())
                {
                    exists = reader.Read();
                }
                if (!exists)
                {
                    cmd.CommandText = table.Sql;
                    try { cmd.ExecuteNonQuery(); Console.WriteLine($"[DB INIT] Table '{table.Name}' created."); } catch (Exception ex) { Console.WriteLine($"[DB INIT ERROR] Table '{table.Name}' creation failed: {ex.Message}"); }
                }
                else
                {
                    Console.WriteLine($"[DB INIT] Table '{table.Name}' already exists.");
                }
            }

            // Index creation validation
            var indexDefinitions = new[] {
                new { Name = "IX_CHAT_MESSAGE_SENDER_CUST", Sql = "CREATE INDEX IX_CHAT_MESSAGE_SENDER_CUST ON CHAT_MESSAGE (SENDER_CUSTOMER_CODE)" },
                new { Name = "IX_CHAT_MESSAGE_SENDER_ADMIN", Sql = "CREATE INDEX IX_CHAT_MESSAGE_SENDER_ADMIN ON CHAT_MESSAGE (SENDER_ADMIN_CODE)" }
            };
            foreach (var index in indexDefinitions)
            {
                cmd.CommandText = $"SELECT 1 FROM RDB$INDICES WHERE RDB$INDEX_NAME = '{index.Name}'";
                var exists = false;
                using (var reader = cmd.ExecuteReader())
                {
                    exists = reader.Read();
                }
                if (!exists)
                {
                    cmd.CommandText = index.Sql;
                    try { cmd.ExecuteNonQuery(); Console.WriteLine($"[DB INIT] Index '{index.Name}' created."); } catch (Exception ex) { Console.WriteLine($"[DB INIT ERROR] Index '{index.Name}' creation failed: {ex.Message}"); }
                }
                else
                {
                    Console.WriteLine($"[DB INIT] Index '{index.Name}' already exists.");
                }
            }

            // Triggers
            // PK triggers
            cmd.CommandText = @"SELECT 1 FROM RDB$TRIGGERS WHERE RDB$TRIGGER_NAME = 'TRG_CHAT_MESSAGE_PK'";
            var pkTriggerExists = false;
            using (var reader = cmd.ExecuteReader())
            {
                pkTriggerExists = reader.Read();
            }
            if (!pkTriggerExists)
            {
                var pkTriggers = new[] {
                    @"CREATE OR ALTER TRIGGER TRG_CHAT_MESSAGE_PK
BEFORE INSERT ON CHAT_MESSAGE
AS
BEGIN
  IF (NEW.MSG_ID IS NULL) THEN
    NEW.MSG_ID = NEXT VALUE FOR SEQ_CHAT_MESSAGE;
END",
                    @"CREATE OR ALTER TRIGGER TRG_QUOTATION_REQUEST_PK
BEFORE INSERT ON QUOTATION_REQUEST
AS
BEGIN
  IF (NEW.REQUEST_ID IS NULL) THEN
    NEW.REQUEST_ID = NEXT VALUE FOR SEQ_QUOTATION_REQUEST;
END",
                    @"CREATE OR ALTER TRIGGER TRG_QUOTATION_REQUEST_ITEM_PK
BEFORE INSERT ON QUOTATION_REQUEST_ITEM
AS
BEGIN
  IF (NEW.REQUEST_ITEM_ID IS NULL) THEN
    NEW.REQUEST_ITEM_ID = NEXT VALUE FOR SEQ_QUOTATION_REQUEST_ITEM;
END"
                };
                foreach (var trig in pkTriggers)
                {
                    cmd.CommandText = trig;
                    try { cmd.ExecuteNonQuery(); Console.WriteLine("[DB INIT] PK trigger created."); } catch (Exception ex) { Console.WriteLine($"[DB INIT ERROR] PK trigger creation failed: {ex.Message}"); }
                }
            }
            else
            {
                Console.WriteLine("[DB INIT] PK triggers already exist.");
            }

            // Sender snapshot trigger
            cmd.CommandText = @"SELECT 1 FROM RDB$TRIGGERS WHERE RDB$TRIGGER_NAME = 'TRG_CHAT_MESSAGE_SENDER_SNAPSHOT'";
            var senderTriggerExists = false;
            using (var reader = cmd.ExecuteReader())
            {
                senderTriggerExists = reader.Read();
            }
            if (!senderTriggerExists)
            {
                cmd.CommandText = @"CREATE OR ALTER TRIGGER TRG_CHAT_MESSAGE_SENDER_SNAPSHOT
ACTIVE BEFORE INSERT OR UPDATE
ON CHAT_MESSAGE
AS
DECLARE VARIABLE vName VARCHAR(200);
BEGIN
IF (NEW.SENDER_NAME_SNAPSHOT IS NULL OR TRIM(NEW.SENDER_NAME_SNAPSHOT) = '') THEN
BEGIN
  IF (NEW.SENDER_TYPE = 'bot') THEN
    NEW.SENDER_NAME_SNAPSHOT = 'BOT';
  ELSE IF (NEW.SENDER_TYPE = 'customer' AND NEW.SENDER_CUSTOMER_CODE IS NOT NULL AND TRIM(NEW.SENDER_CUSTOMER_CODE) <> '') THEN
  BEGIN
    vName = NULL;
    SELECT FIRST 1 TRIM(c.COMPANYNAME)
      FROM AR_CUSTOMER c
     WHERE c.CODE = NEW.SENDER_CUSTOMER_CODE
     INTO vName;
    IF (vName IS NOT NULL AND TRIM(vName) <> '') THEN
      NEW.SENDER_NAME_SNAPSHOT = vName;
    ELSE
      NEW.SENDER_NAME_SNAPSHOT = NEW.SENDER_CUSTOMER_CODE;
  END
  ELSE IF (NEW.SENDER_TYPE = 'admin' AND NEW.SENDER_ADMIN_CODE IS NOT NULL AND TRIM(NEW.SENDER_ADMIN_CODE) <> '') THEN
  BEGIN
    vName = NULL;
    SELECT FIRST 1 TRIM(u.NAME)
      FROM SY_USER u
     WHERE u.CODE = NEW.SENDER_ADMIN_CODE
     INTO vName;
    IF (vName IS NOT NULL AND TRIM(vName) <> '') THEN
      NEW.SENDER_NAME_SNAPSHOT = vName;
    ELSE
      NEW.SENDER_NAME_SNAPSHOT = NEW.SENDER_ADMIN_CODE;
  END
END
END";
                try { cmd.ExecuteNonQuery(); Console.WriteLine("[DB INIT] Sender snapshot trigger created."); } catch (Exception ex) { Console.WriteLine($"[DB INIT ERROR] Sender snapshot trigger creation failed: {ex.Message}"); }
            }
            else
            {
                Console.WriteLine("[DB INIT] Sender snapshot trigger already exists.");
            }

            Console.WriteLine("[DB INIT] Table creation and triggers executed.");
        }
    }
}
