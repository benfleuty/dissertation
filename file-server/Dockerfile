FROM ubuntu:latest

RUN apt update -y
RUN apt install vsftpd -y

RUN mkdir -p /fileserver/files
RUN mkdir -p /var/run/vsftpd/empty

RUN chown root:root /var/run/vsftpd/empty
RUN chmod 555 /var/run/vsftpd/empty

RUN addgroup ftp-data

RUN adduser --disabled-password --gecos '' ftpuser
RUN echo 'ftpuser:gwmxrpUH6cXTM5rC' | chpasswd
RUN usermod -d /fileserver/files ftpuser
RUN usermod -s /sbin/nologin ftpuser

RUN chown -R ftpuser:ftp-data /fileserver/files
RUN chmod -R 777 /fileserver/files

COPY vsftpd.conf /etc/vsftpd.conf
COPY vsftpd.userlist /etc/vsftpd.userlist
COPY vsftpd.chroot_list /etc/vsftpd.chroot_list

VOLUME [ "/uploads-volume" ]

EXPOSE 20 21 21100-21110

CMD ["vsftpd"]
