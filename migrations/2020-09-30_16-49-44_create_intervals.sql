CREATE TABLE public.intervals
(
    channel_name text NOT NULL,
    alias text NOT NULL,
    message text NOT NULL,
    "time" bigint NOT NULL,
    PRIMARY KEY (channel_name),
    CONSTRAINT "FK_intervals_channel_info_name" FOREIGN KEY (channel_name)
        REFERENCES public.channel_info (name) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
        NOT VALID
);

ALTER TABLE public.intervals
    OWNER to postgres;
