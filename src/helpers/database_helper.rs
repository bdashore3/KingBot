use dashmap::DashMap;
use sqlx::postgres::{PgPoolOptions, PgPool};
use std::error::Error;

use crate::structures::KingResult;

pub async fn obtain_db_pool(db_connection: &str) -> Result<PgPool, Box<dyn Error + Send + Sync>> {
    let pool = PgPoolOptions::new()
        .max_connections(10)
        .connect(&db_connection).await?;
    
    Ok(pool)
}

pub async fn fetch_prefixes(pool: &PgPool) -> KingResult<DashMap<String, String>> {
    let prefixes: DashMap<String, String> = DashMap::new();
    
    let cursor = sqlx::query!("SELECT name, prefix FROM channel_info")
        .fetch_all(pool).await?;
    
    for i in cursor {
        if let Some(prefix) = i.prefix {
            prefixes.insert(i.name, prefix);
        }
    }
    
    Ok(prefixes)
}
