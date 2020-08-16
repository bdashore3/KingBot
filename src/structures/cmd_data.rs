use typemap_rev::TypeMapKey;
use std::{sync::Arc, collections::HashMap};
use sqlx::PgPool;
use dashmap::DashMap;

pub struct PubCreds;

impl TypeMapKey for PubCreds {
    type Value = Arc<HashMap<String, String>>;
}

pub struct ConnectionPool;

impl TypeMapKey for ConnectionPool {
    type Value = Arc<PgPool>;
}

pub struct LurkTimes;

impl TypeMapKey for LurkTimes {
    type Value = Arc<DashMap<String, u64>>;
}