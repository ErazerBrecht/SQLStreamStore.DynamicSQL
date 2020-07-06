# SQLStreamStore.DynamicSQL
Bug repro for 'slow subscribe' when using PostgresSQL implemantion

## Proposed fix:
```
CREATE OR REPLACE FUNCTION public.read_all(
	_count integer, 
	_position bigint, 
	_forwards boolean, 
	_prefetch boolean
)
  RETURNS TABLE(
    stream_id      VARCHAR(1000),
    message_id     UUID,
    stream_version INT,
    "position"     BIGINT,
    create_utc     TIMESTAMP,
    "type"         VARCHAR(128),
    json_metadata  JSONB,
    json_data      JSONB,
    max_age        INT
  )
LANGUAGE 'plpgsql'
AS $function$
BEGIN
  
  RETURN QUERY EXECUTE'
  WITH messages AS (
      SELECT public.streams.id_original,
             public.messages.message_id,
             public.messages.stream_version,
             public.messages.position,
             public.messages.created_utc,
             public.messages.type,
             public.messages.json_metadata,
             (CASE $1
                WHEN TRUE THEN public.messages.json_data
                ELSE NULL END),
             public.streams.max_age
      FROM public.messages
             INNER JOIN public.streams ON public.messages.stream_id_internal = public.streams.id_internal
      WHERE (CASE
               WHEN $2 THEN public.messages.position >= $3
               ELSE public.messages.position <= $3 END)
      ORDER BY (CASE
                  WHEN $2 THEN public.messages.position
                  ELSE -public.messages.position END)
  )
  SELECT * FROM messages LIMIT $4;'
  USING _prefetch, _forwards, _position, _count;
END;
$function$;
```
