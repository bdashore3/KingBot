-- Add migration script here
CREATE TABLE public.commands
(
    channel_name text NOT NULL,
    name text NOT NULL,
    content text NOT NULL,
    PRIMARY KEY (channel_name, name),
    CONSTRAINT "FK_commands_channel_info_name" FOREIGN KEY (channel_name)
        REFERENCES public.channel_info (name) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

ALTER TABLE public.commands
    OWNER to postgres;