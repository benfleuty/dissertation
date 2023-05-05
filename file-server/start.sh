#!/bin/sh

# Redirect xferlog output to stdout
sed -i 's|/var/log/vsftpd.log|/dev/stdout|g' /etc/vsftpd/vsftpd.conf

# Start VSFTPD with the modified config file
vsftpd /etc/vsftpd/vsftpd.conf

# Redirect stdout to a log file
tail -f /dev/null > /var/log/vsftpd.log