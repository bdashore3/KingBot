use std::{collections::HashMap, sync::Arc};
use dashmap::DashMap;
use typemap_rev::TypeMapKey;
use sqlx::PgPool;
use tokio::sync::RwLock;
use super::IntervalInfo;

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

pub struct IntervalMap;

impl TypeMapKey for IntervalMap {
    type Value = Arc<RwLock<Vec<IntervalInfo>>>;
}
