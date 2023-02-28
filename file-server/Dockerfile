FROM alpine:latest

RUN apk update && \
    apk add vsftpd && \
    apk add openssl && \
    rm -rf /var/cache/apk/*

RUN adduser -D -h /ftp -s /sbin/nologin -u 1001 --ingroup ftp ftpuser && \
    echo "ftpuser:ftppassword" | chpasswd

RUN openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -subj "/C=US/ST=CA/L=San Francisco/O=My Org/CN=localhost" \
    -keyout /etc/vsftpd/vsftpd.key -out /etc/vsftpd/vsftpd.crt

RUN mkdir -p /var/log/vsftpd
RUN touch /var/log/vsftpd/vsftpd.log
RUN chown -R ftp:ftp /var/log/vsftpd
RUN chmod 777 /var/log/vsftpd/vsftpd.log

RUN mkdir -p /ftp/ftpuser && \
    chown ftpuser:ftp /ftp/ftpuser && \
    chmod 750 /ftp/ftpuser

COPY vsftpd.conf /etc/vsftpd/

EXPOSE 20 21 21100-21110

CMD ["vsftpd", "/etc/vsftpd/vsftpd.conf"]
