-- Add migration script here
CREATE TABLE public.messages
(
    channel_name text NOT NULL,
    shoutout text,
    PRIMARY KEY (channel_name),
    CONSTRAINT "FK_messages_channel_info_name" FOREIGN KEY (channel_name)
        REFERENCES public.channel_info (name) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
        NOT VALID
);

ALTER TABLE public.messages
    OWNER to postgres;
