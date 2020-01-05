sudo apt update
sudo apt install
sudo apt install docker
sudo apt install docker.io
sudo apt install python-pip
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
az extension add --name azure-cli-iot-ext
sudo apt install nodejs
sudo apt install npm
sudo npm i -g iothub-explorer
sudo pip install -U docker-compose
sudo pip install -U iotedgedev
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo add-apt-repository universe
sudo apt-get update
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.2
sudo groupadd docker
sudo usermod -aG docker $USER
sudo apt-get install -y qemu qemu-user-static qemu-user binfmt-support
sudo mkdir -p /lib/binfmt.d
sudo sh -c 'echo :qemu-arm:M::\\x7fELF\\x01\\x01\\x01\\x00\\x00\\x00\\x00\\x00\\x00\\x00\\x00\\x00\\x02\\x00\\x28\\x00:\\xff\\xff\\xff\\xff\\xff\\xff\\xff\\x00\\x00\\x00\\x00\\x00\\x00\\x00\\x00\\x00\\xfe\\xff\\xff\\xff:/usr/bin/qemu-arm-static:F > /lib/binfmt.d/qemu-arm-static.conf'
sudo sh -c 'echo :qemu-aarch64:M::\\x7fELF\\x02\\x01\\x01\\x00\\x00\\x00\\x00\\x00\\x00\\x00\\x00\\x00\\x02\\x00\\xb7\\x00:\\xff\\xff\\xff\\xff\\xff\\xff\\xff\\x00\\x00\\x00\\x00\\x00\\x00\\x00\\x00\\x00\\xfe\\xff\\xff\\xff:/usr/bin/qemu-aarch64-static:F > /lib/binfmt.d/qemu-aarch64-static.conf'
sudo systemctl restart systemd-binfmt.service