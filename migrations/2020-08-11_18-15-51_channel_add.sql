-- Add migration script here
CREATE TABLE public.channel_info
(
    name text COLLATE pg_catalog."default" NOT NULL,
    prefix text COLLATE pg_catalog."default",
    CONSTRAINT channel_info_pkey PRIMARY KEY (name)
)

TABLESPACE pg_default;

ALTER TABLE public.channel_info
    OWNER to postgres;
