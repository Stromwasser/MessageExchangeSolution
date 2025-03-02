CREATE TABLE IF NOT EXISTS public.messages
(
    id SERIAL PRIMARY KEY,
    text VARCHAR(128) NOT NULL,
    createdat TIMESTAMP DEFAULT now(),
    sequencenumber INT NOT NULL
);
INSERT INTO public.messages (text, createdat, sequencenumber) VALUES
('Hello, world!', now(), 1),
('Test message 1', now(), 2),
('Test message 2', now(), 3);