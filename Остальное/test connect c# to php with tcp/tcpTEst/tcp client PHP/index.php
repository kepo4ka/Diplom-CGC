<?php

$socket = socket_create(AF_INET, SOCK_STREAM, 0); // Создаем TCP Socket

$ret = socket_connect($socket, "127.0.0.1", 9595); // Устанавливаем соединение с портом 1234 хоста 198.51.100.1
if(!$ret): print("error connecting"); die(); endif;

$ret = socket_send($socket, "я есть пхъп", 4096, 0); // Отправляем $length байт из $bindata

$ret = socket_read($socket,4096, 0);
echo $ret;
//
//if($ret!=$length): print("Error send data"); die(); endif;
socket_close($socket); // Закрываем сокет

?>