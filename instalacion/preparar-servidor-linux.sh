#!/bin/bash
# FUNBIDE - preparar esta maquina como servidor en su red DEFINITIVA.
#
# Correr UNA sola vez, el dia de la instalacion final, con esta maquina ya
# conectada por cable al switch/red donde va a quedar para siempre:
#
#   sudo bash instalacion/preparar-servidor-linux.sh
#
# Que hace:
#   1. Detecta la IP/gateway/DNS que le asigno el DHCP de esa red en este
#      momento, y la fija como estatica (para que no cambie mas).
#   2. Si el firewall (ufw) esta activo, abre el puerto de FUNBIDE.
#   3. Muestra la URL a la que se tienen que conectar las demas PCs.
#
# Docker y los contenedores de FUNBIDE ya estan configurados para arrancar
# solos con la maquina (systemd + restart:unless-stopped) - no hace falta
# nada mas aparte de este script.
#
# Se puede correr mas de una vez sin problema (por ejemplo si el switch
# definitivo cambia de lugar/red): cada corrida vuelve a fijar la IP que
# tenga en ese momento.

set -euo pipefail

if [ "$(id -u)" -ne 0 ]; then
  echo "Hace falta correrlo con sudo (root): sudo bash $0" >&2
  exit 1
fi

PUERTO="${FUNBIDE_PUERTO:-8080}"

IFAZ=$(ip route show default | awk '{print $5; exit}')
if [ -z "$IFAZ" ]; then
  echo "No se detecto una interfaz con salida a internet/gateway." >&2
  echo "Conecta el cable de red al switch definitivo y volve a correr este script." >&2
  exit 1
fi

CONEXION=$(nmcli -t -f DEVICE,CONNECTION device status | awk -F: -v d="$IFAZ" '$1==d{print $2; exit}')
if [ -z "$CONEXION" ] || [ "$CONEXION" = "--" ]; then
  echo "No se encontro un perfil de NetworkManager activo para la interfaz $IFAZ." >&2
  exit 1
fi

IP_ACTUAL=$(ip -4 -o addr show dev "$IFAZ" | awk '{print $4; exit}')
GATEWAY=$(ip route show default dev "$IFAZ" | awk '{print $3; exit}')
DNS=$(resolvectl dns "$IFAZ" 2>/dev/null | sed 's/^[^:]*: //' | tr -s ' ' ',')

if [ -z "$IP_ACTUAL" ] || [ -z "$GATEWAY" ]; then
  echo "No se pudo leer la IP/gateway actual por DHCP." >&2
  echo "Confirma que el cable este conectado al switch definitivo y que ese switch/router reparte DHCP." >&2
  exit 1
fi

echo "Interfaz de red: $IFAZ (perfil NetworkManager: $CONEXION)"
echo "Fijando como estatica: $IP_ACTUAL  gateway $GATEWAY  DNS ${DNS:-sin cambios}"
echo

nmcli connection modify "$CONEXION" \
  ipv4.method manual \
  ipv4.addresses "$IP_ACTUAL" \
  ipv4.gateway "$GATEWAY" \
  ${DNS:+ipv4.dns "$DNS"}

nmcli connection up "$CONEXION" >/dev/null

if command -v ufw >/dev/null 2>&1 && ufw status | grep -q "Status: active"; then
  echo "ufw esta activo: abriendo el puerto $PUERTO/tcp"
  ufw allow "$PUERTO"/tcp
fi

IP_SIN_MASCARA="${IP_ACTUAL%%/*}"

echo
echo "======================================================================"
echo " Listo. IP fija de este servidor: $IP_SIN_MASCARA"
echo
echo " Las demas PCs de la red se conectan abriendo el navegador en:"
echo "     http://$IP_SIN_MASCARA:$PUERTO"
echo
echo " Sugerencia (opcional, mas seguro): reservar $IP_SIN_MASCARA para la"
echo " direccion fisica (MAC) de esta maquina en el router/switch, para que"
echo " nunca se la asignen a otro equipo. La MAC de $IFAZ es:"
echo "     $(cat /sys/class/net/"$IFAZ"/address 2>/dev/null || echo 'no disponible')"
echo "======================================================================"
