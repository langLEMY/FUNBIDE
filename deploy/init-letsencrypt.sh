#!/usr/bin/env bash
set -euo pipefail

# Bootstrap de certificados TLS de Let's Encrypt.
#
# Correr UNA SOLA VEZ en un servidor nuevo, antes del primer "docker compose up"
# real: Nginx no arranca sin certificados válidos en /etc/letsencrypt/live/<dominio>,
# y certbot no puede emitir el certificado real sin que Nginx ya esté sirviendo
# el challenge HTTP-01 en el puerto 80. Este script resuelve ese círculo:
# certificado autofirmado temporal -> arrancar Nginx -> pedir el certificado
# real por webroot -> recargar Nginx.
#
# Uso: deploy/init-letsencrypt.sh [dominio] <email-de-contacto>

domain="${1:-api.funbide.org}"
email="${2:?Uso: deploy/init-letsencrypt.sh [dominio] <email-de-contacto>}"
rsa_key_size=4096
cert_path="/etc/letsencrypt/live/$domain"

echo "### Creando certificado autofirmado temporal para $domain ..."
docker compose run --rm --entrypoint \
  "sh -c \"mkdir -p '$cert_path' && openssl req -x509 -nodes -newkey rsa:$rsa_key_size -days 1 -keyout '$cert_path/privkey.pem' -out '$cert_path/fullchain.pem' -subj '/CN=$domain'\"" \
  certbot

echo "### Arrancando Nginx con el certificado temporal ..."
docker compose up -d --force-recreate nginx

echo "### Eliminando el certificado temporal ..."
docker compose run --rm --entrypoint \
  "sh -c \"rm -rf $cert_path /etc/letsencrypt/archive/$domain /etc/letsencrypt/renewal/$domain.conf\"" \
  certbot

echo "### Solicitando el certificado real a Let's Encrypt ..."
docker compose run --rm --entrypoint \
  "certbot certonly --webroot -w /var/www/certbot -d $domain --email $email --rsa-key-size $rsa_key_size --agree-tos --non-interactive" \
  certbot

echo "### Recargando Nginx con el certificado real ..."
docker compose exec nginx nginx -s reload

echo "Listo. Verificá https://$domain/health"
