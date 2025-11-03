cat > generate-kafka-ssl.sh <<'EOF'

#!/bin/bash


# ==============================
# Tạo SSL cho Kafka với CA riêng
# ==============================

# Thư mục lưu cert
CERT_DIR=./certs
mkdir -p $CERT_DIR

# Các file quan trọng
KEYSTORE=$CERT_DIR/kafka.server.keystore.jks
TRUSTSTORE=$CERT_DIR/kafka.server.truststore.jks
CA_KEY=$CERT_DIR/ca-key.pem
CA_CERT=$CERT_DIR/ca-cert.pem
CSR_FILE=$CERT_DIR/kafka.csr
SIGNED_CERT=$CERT_DIR/kafka-signed.pem

# Mật khẩu
KEYSTORE_PASS="hKq5xu7tXlSAq9Yxlq7OnDUMIVl8gugM"
KEY_PASS="hKq5xu7tXlSAq9Yxlq7OnDUMIVl8gugM"
TRUSTSTORE_PASS="hKq5xu7tXlSAq9Yxlq7OnDUMIVl8gugM"

# Thông tin CN & SAN
CN_NAME="217.217.254.66" # hoặc hostname thực tế của Kafka
SAN_DNS="kafka"
SAN_IP="217.217.254.66"

echo "=== Tạo file cấu hình SAN tạm thời ==="
cat > $CERT_DIR/ext.cnf <<EOCNF
[ req ]
distinguished_name = dn
x509_extensions = v3_req
prompt = no

[ dn ]
CN = $CN_NAME

[ v3_req ]
subjectAltName = @alt_names

[ alt_names ]
DNS.1 = $SAN_DNS
IP.1 = $SAN_IP
EOCNF


echo "=== Bước 2: Tạo Certificate Authority (CA) ==="
openssl genrsa -out $CA_KEY 4096
openssl req -x509 -new -nodes -key $CA_KEY -sha256 -days 3650 \
  -out $CA_CERT \
  -subj "/C=VN/ST=HCM/L=Ho Chi Minh/O=TechLand/OU=IT/CN=TechLand-CA"

echo "=== Bước 3: Tạo Kafka Keystore (chứa private key) ==="
keytool -genkey \
  -alias $CN_NAME \
  -keystore $KEYSTORE \
  -keyalg RSA \
  -keysize 2048 \
  -validity 365 \
  -storepass $KEYSTORE_PASS \
  -keypass $KEY_PASS \
  -dname "CN=$CN_NAME, OU=IT, O=TechLand, L=Ho Chi Minh, S=HCM, C=VN" \
  -ext SAN=dns:$SAN_DNS,ip:$SAN_IP

echo "=== Bước 4: Sinh CSR từ Keystore ==="
keytool -certreq \
  -alias $CN_NAME \
  -keystore $KEYSTORE \
  -file $CSR_FILE \
  -storepass $KEYSTORE_PASS

echo "=== Bước 5: Ký CSR bằng CA để tạo Kafka cert ==="
openssl x509 -req -in $CSR_FILE -CA $CA_CERT -CAkey $CA_KEY -CAcreateserial \
  -out $SIGNED_CERT -days 365 -sha256 -extfile $CERT_DIR/ext.cnf -extensions v3_req

echo "=== Bước 6: Import CA vào Keystore ==="
keytool -importcert \
  -trustcacerts \
  -alias CARoot \
  -file $CA_CERT \
  -keystore $KEYSTORE \
  -storepass $KEYSTORE_PASS \
  -noprompt

echo "=== Bước 7: Import Kafka cert đã ký vào Keystore ==="
keytool -importcert \
  -alias $CN_NAME \
  -file $SIGNED_CERT \
  -keystore $KEYSTORE \
  -storepass $KEYSTORE_PASS \
  -noprompt

echo "=== Bước 8: Tạo Truststore và import CA ==="
keytool -importcert \
  -alias CARoot \
  -file $CA_CERT \
  -keystore $TRUSTSTORE \
  -storepass $TRUSTSTORE_PASS \
  -noprompt

echo "=== Bước 9: Tạo file mật khẩu (credentials) ==="
echo "$KEYSTORE_PASS" > $CERT_DIR/keystore_creds
echo "$KEY_PASS" > $CERT_DIR/key_creds
echo "$TRUSTSTORE_PASS" > $CERT_DIR/truststore_creds

echo "=== Hoàn tất! Các file trong $CERT_DIR ==="
ls -l $CERT_DIR

echo "=== Hướng dẫn sử dụng cho Kafka ==="
echo "
ssl.keystore.location=$KEYSTORE
ssl.keystore.password=$(cat $CERT_DIR/keystore_creds)
ssl.key.password=$(cat $CERT_DIR/key_creds)
ssl.truststore.location=$TRUSTSTORE
ssl.truststore.password=$(cat $CERT_DIR/truststore_creds)
"

EOF
