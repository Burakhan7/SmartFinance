import 'package:flutter/material.dart';
import '../models/offer.dart';
import '../services/api_service.dart';

class OffersScreen extends StatefulWidget {
  const OffersScreen({super.key});

  @override
  State<OffersScreen> createState() => _OffersScreenState();
}

class _OffersScreenState extends State<OffersScreen> {
  final _api = ApiService();
  // Test için sabit bir müşteri id (ileride gerçek kullanıcıdan gelecek)
  static const _customerId = '44444444-4444-4444-4444-444444444444';

  List<Offer> _offers = [];
  bool _loading = false;
  bool _connected = false;
  String? _error;

  // "Kartımı Bağla": rıza ver -> sync et -> fırsatları getir
  Future<void> _connectAndLoad() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      await _api.grantConsent(_customerId);
      await _api.sync(_customerId);
      final offers = await _api.getOffers(_customerId);
      setState(() {
        _offers = offers;
        _connected = true;
      });
    } catch (e) {
      setState(() {
        _error = e.toString();
      });
    } finally {
      setState(() {
        _loading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('SmartFinance'),
        backgroundColor: Colors.indigo,
        foregroundColor: Colors.white,
      ),
      body: Padding(padding: const EdgeInsets.all(16), child: _buildBody()),
    );
  }

  Widget _buildBody() {
    if (_loading) {
      return const Center(child: CircularProgressIndicator());
    }
    if (_error != null) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.error_outline, size: 48, color: Colors.red),
            const SizedBox(height: 12),
            Text('Hata: $_error', textAlign: TextAlign.center),
            const SizedBox(height: 12),
            ElevatedButton(
              onPressed: _connectAndLoad,
              child: const Text('Tekrar Dene'),
            ),
          ],
        ),
      );
    }
    if (!_connected) {
      // Başlangıç ekranı: kart bağlama daveti
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(
              Icons.account_balance_wallet,
              size: 64,
              color: Colors.indigo,
            ),
            const SizedBox(height: 16),
            const Text(
              'Kartını bağla, harcamalarını\nanaliz edip sana özel fırsatlar sunalım.',
              textAlign: TextAlign.center,
              style: TextStyle(fontSize: 16),
            ),
            const SizedBox(height: 24),
            ElevatedButton.icon(
              onPressed: _connectAndLoad,
              icon: const Icon(Icons.link),
              label: const Text('Kartımı Bağla'),
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.indigo,
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(
                  horizontal: 32,
                  vertical: 16,
                ),
              ),
            ),
          ],
        ),
      );
    }
    // Bağlandıktan sonra: fırsat listesi
    return ListView.builder(
      itemCount: _offers.length,
      itemBuilder: (context, i) => _offerCard(_offers[i]),
    );
  }

  Widget _offerCard(Offer offer) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      elevation: 2,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
              decoration: BoxDecoration(
                color: Colors.indigo.shade50,
                borderRadius: BorderRadius.circular(12),
              ),
              child: Text(
                offer.typeLabel,
                style: TextStyle(
                  color: Colors.indigo.shade700,
                  fontWeight: FontWeight.w600,
                  fontSize: 12,
                ),
              ),
            ),
            const SizedBox(height: 8),
            Text(
              offer.title,
              style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 6),
            Text(
              offer.description,
              style: TextStyle(fontSize: 14, color: Colors.grey.shade700),
            ),
          ],
        ),
      ),
    );
  }
}
