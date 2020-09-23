use std::{collections::HashMap, sync::Arc};
use dashmap::DashMap;
use typemap_rev::TypeMapKey;
use sqlx::PgPool;

pub struct ConnectionPool;

impl TypeMapKey for ConnectionPool {
    type Value = PgPool;
}

pub struct PrefixMap;

impl TypeMapKey for PrefixMap {
    type Value = Arc<DashMap<String, String>>;
}

pub struct PubCreds;

impl TypeMapKey for PubCreds {
    type Value = Arc<HashMap<String, String>>;
}
