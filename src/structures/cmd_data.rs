use typemap_rev::TypeMapKey;
use std::{sync::Arc, collections::HashMap};
use sqlx::PgPool;

pub struct PubCreds;

impl TypeMapKey for PubCreds {
    type Value = Arc<HashMap<String, String>>;
}

pub struct ConnectionPool;

impl TypeMapKey for ConnectionPool {
    type Value = Arc<PgPool>;
}