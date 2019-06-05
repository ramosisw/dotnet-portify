SCRIPT=$(readlink -f $0)
SCRIPT_PATH=`dirname $SCRIPT`
echo $SCRIPT_PATH
sudo docker-compose -f $SCRIPT_PATH/docker-compose.yml stop
sudo docker system prune -af
sudo docker-compose -f $SCRIPT_PATH/docker-compose.yml  up -d
