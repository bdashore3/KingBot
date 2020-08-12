use sqlx::postgres::{PgPoolOptions, PgPool};
use std::error::Error;

pub async fn obtain_db_pool(db_connection: String) -> Result<PgPool, Box<dyn Error + Send + Sync>> {

    let connection_string = &db_connection;

    let pool = PgPoolOptions::new()
        .max_connections(10)
        .connect(&connection_string).await?;
    
    Ok(pool)
}