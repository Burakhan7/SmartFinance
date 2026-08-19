import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/offer.dart';

class ApiService {
  // Android emülatör için bilgisayarın localhost'u = 10.0.2.2
  static const String baseUrl = 'http://10.0.2.2:5283/api';

  // Rıza ver
  Future<void> grantConsent(String customerId) async {
    final res = await http.post(
      Uri.parse('$baseUrl/customers/$customerId/consent'),
    );
    if (res.statusCode != 200) {
      throw Exception('Rıza verilemedi: ${res.statusCode}');
    }
  }

  // Kart hareketlerini senkronize et (rıza sonrası)
  Future<int> sync(String customerId) async {
    final res = await http.post(Uri.parse('$baseUrl/sync/$customerId'));
    if (res.statusCode != 200) {
      throw Exception('Senkronizasyon başarısız: ${res.statusCode}');
    }
    final data = jsonDecode(res.body);
    return data['eklenenHareket'] as int;
  }

  // Fırsatları getir (yoksa backend otomatik üretir)
  Future<List<Offer>> getOffers(String customerId) async {
    final res = await http.get(
      Uri.parse('$baseUrl/customers/$customerId/offers'),
    );
    if (res.statusCode != 200) {
      throw Exception('Fırsatlar alınamadı: ${res.statusCode}');
    }
    final List<dynamic> data = jsonDecode(res.body);
    return data.map((j) => Offer.fromJson(j)).toList();
  }
}
