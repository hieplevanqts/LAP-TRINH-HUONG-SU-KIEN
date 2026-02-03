# Phan tich nghiep vu - Quan ly khach san

## Pham vi va muc tieu
- Quan ly phong, loai phong, tinh trang phong theo thoi gian.
- Quan ly dat phong, check-in/check-out, phan bo phong.
- Quan ly khach hang, nhan vien, tai khoan dang nhap va phan quyen.
- Quan ly dich vu (an uong, giat ui, dua don, ...), ghi nhan su dung.
- Lap hoa don, thanh toan, ap dung thue/khuyen mai.
- Bao cao doanh thu va thong ke cong suat phong.

## Tac nhan (Actors)
- Admin: quan tri he thong, phan quyen, cau hinh.
- Quan ly (Manager): giam sat hoat dong, bao cao.
- Le tan (Receptionist): dat phong, check-in/out, khach hang.
- Ke toan (Accountant): hoa don, thanh toan, bao cao doanh thu.

## Cac phan he chinh
- Danh muc: Loai phong, Phong, Dich vu, Gia phong.
- Nhan su/KH: Khach hang, Nhan vien, Tai khoan.
- Dat phong: Dat phong, Gan phong, Check-in/out.
- Dich vu: Ghi nhan su dung dich vu theo dat phong.
- Hoa don & Thanh toan.
- Bao cao: doanh thu theo ngay/thang, cong suat phong.

## Quy trinh tong quan
1. Tao/Cap nhat khach hang.
2. Tao dat phong (Booking), chon khoang thoi gian.
3. Gan phong (BookingRooms) theo tinh trang phong.
4. Check-in: xac nhan nhan phong.
5. Trong qua trinh o: them dich vu (ServiceUsages).
6. Check-out: tinh tien phong + dich vu, lap hoa don.
7. Thu tien va in hoa don.
8. Tong hop bao cao theo ngay/thang.
