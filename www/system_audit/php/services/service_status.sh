#!/bin/bash

# Script que executa el php check_status cada cop que s'encèn el servidor de manera infinita, fent una pausa de 5 segons.

while true
do
php /var/www/system_audit/php/services/check_status.php
sleep 5
done
