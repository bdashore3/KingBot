-- Add migration script here
CREATE TABLE public.quotes
(
    channel_name text NOT NULL,
    alias text NOT NULL,
    content text NOT NULL,
    PRIMARY KEY (alias, channel_name),
    CONSTRAINT "FK_quotes_channel_info_name" FOREIGN KEY (channel_name)
        REFERENCES public.channel_info (name) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
        NOT VALID
);

ALTER TABLE public.quotes
    OWNER to postgres;