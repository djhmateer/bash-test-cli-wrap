#!/bin/sh

# check to see if this is a brand new build, or we're taking from our built image
# which for some reason runs the cloud-init script too
filename="/home/dave/facesearch/1.sh"
now=$(date +"%T")
if [ -f "$filename" ]; then
  echo "doing nothing at $now" >> /var/log/myScript.txt
else
  echo "Start cloud-init at $now" >> /var/log/myScript.txt
  START=$(date +%s.%N)

  # go with newer apt which gets dependency updates too (like linux-azure)
  sudo apt update -y
  sudo apt upgrade -y

  # CUDA - library to handle NVIDIA GPUS 4.4GB
  sudo apt install nvidia-cuda-toolkit -y

  # CUDA Ubuntu
  wget https://developer.download.nvidia.com/compute/cuda/repos/ubuntu2004/x86_64/cuda-ubuntu2004.pin
  sudo mv cuda-ubuntu2004.pin /etc/apt/preferences.d/cuda-repository-pin-600
  sudo apt-key adv --fetch-keys https://developer.download.nvidia.com/compute/cuda/repos/ubuntu2004/x86_64/7fa2af80.pub
  sudo add-apt-repository "deb https://developer.download.nvidia.com/compute/cuda/repos/ubuntu2004/x86_64/ /"
  sudo apt update -y

  # LibCuda8 need this version or get strange errors with nvtop
  sudo apt install libcudnn8=8.1.1.*-1+cuda11.2 -y
  sudo apt install libcudnn8-dev=8.1.1.*-1+cuda11.2 -y

  # # NVidia driver 900MB
  sudo apt install nvidia-driver-465 -y

  # do I need this???
  # export CUDA_PATH=/usr
  # source ~/.bashrc

  # # DLib 11MB
  cd /home/dave
  wget http://dlib.net/files/dlib-19.21.tar.bz2
  tar jxvf dlib-19.21.tar.bz2
  rm dlib-19.21.tar.bz2
  cd dlib-19.21

  # need cmake to compile dlib
  sudo apt install cmake -y
  # need this to compile dlib optimally
  sudo apt install libopenblas-dev liblapack-dev libjpeg-dev -y
  # unknown if I need this now, but will further down
  sudo apt install python3-dev python3-numpy -y
  sudo CC=gcc-8 python3 setup.py install clean

  # FaceSearch Python dependencies
  sudo apt install python3-pip -y
  pip3 install face_recognition
  pip3 install pdfkit

  # not sure what Phil wants to do here
  #sudo apt install wkhtmltopdf -y

  # FaceSearch postgres - this libpq-dev is needed for psycopg2
  # hope we don't need postgres anymore on the T4?
  sudo apt install libpq-dev -y
  pip3 install psycopg2

  # FaceSearch_cloud dependencies
  pip3 install imutils
  sudo apt install python3-opencv -y
  pip3 install sklearn
  pip3 install pandas

  # FaceSearch source (Python)
  cd /home/dave/
  sudo git clone https://github.com/spatial-intelligence/OSR4Rights facesearch
  sudo chmod -R 777 facesearch

  # Instructions for FaceSearch_cloud
  # ./faceservice_main.py -i ~/facesearch/job1/ -j 123

  # the input folder needs to contain :       /search       and           /target         with 1+ images in each 
  # it then creates a results folder with thumbnail output of matched images and HTML file
  # and ZIPs that up.. dumping the output in the job folder as results_id.zip (eg results_17.zip)

    # job1
    # included in the repo above
    # 10 random images 

    # job2 - lfw Labelled Faces in the Wild
    # http://vis-www.cs.umass.edu/lfw/
    # 13,233 jpg's 
    # all little 12k  250x250
    # 1680 of the people have 2 or more distinct photos
  # cd facesearch
  # mkdir job2
  # cd job2
  #   # 172MB
  # wget http://vis-www.cs.umass.edu/lfw/lfw.tgz
  # tar -xvf lfw.tgz
  # rm *.tgz
  #   # look in all subdirectories for all files.
  #   # pipe to moves them all to current directory
  #   # - find ~/facesearch/job2/ -type f -print0 | xargs -0 mv -t ~/facesearch/job2
  # find . -type f -print0 | xargs -0 mv -t . 
  # rm -r lfw 
  # cd ..
  # sudo chmod -R 777 job2

    # job3 - Nvidia FFHQ Dataset
    # 155secs to run
    # https://github.com/NVlabs/ffhq-dataset/
    # lots of big pngs
    # 70k images (some huge pngs)
  # cd /home/dave/facesearch
  # mkdir job3
  # cd job3
  # wget https://functionsdm2storage.blob.core.windows.net/outputfiles/images.tgz
  # tar -xvf images.tgz
  # rm *.tgz
  # cp 00136.png target.png
  # cd ..
  # sudo chmod -R 777 job3

    # job4 -  Asian Face Age Dataset
    # takes a while so leaving out
    # http://afad-dataset.github.io/
    # 164,432 images
    # - cd /home/dave/facesearch
    # - git clone https://github.com/afad-dataset/tarball.git job4
    # - cd job4
    # - ./restore.sh
    # - rm -rf *.tar.*
    # - rm README.txt
    # - rm restore.sh
    # - rm -rf .git
    # - find . -type f -print0 | xargs -0 mv -t .
    # - cd ..
    # - chmod -R 777 job4

    # bpytop as a replacement for top. Shows cpu spec and load.
    # https://github.com/aristocratos/bpytop
  # echo "deb http://packages.azlux.fr/debian/ buster main" | sudo tee /etc/apt/sources.list.d/azlux.list
  # wget -qO - https://azlux.fr/repo.gpg.key | sudo apt-key add -
  # sudo apt-get update -y
  # sudo apt-get install bpytop -y

    # nvidia-smi to see GPU usage
    # watch nvidia-ami

    # nvtop for GPU
  # sudo apt install nvtop -y

  now=$(date +"%T")
  echo "End time is ${now}" >> /var/log/myScript.txt

  END=$(date +%s.%N)
  DIFF=$(echo "$END - $START" | bc)
  echo "Total time taken is ${DIFF} seconds" >> /var/log/myScript.txt

  # need to boot for GPU to be recognised
  sudo reboot now
fi