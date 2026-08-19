class Offer {
  final String id;
  final String title;
  final String description;
  final int type; // 0=Insight, 1=Saving, 2=Campaign, 3=Trend
  final bool isSeen;

  Offer({
    required this.id,
    required this.title,
    required this.description,
    required this.type,
    required this.isSeen,
  });

  factory Offer.fromJson(Map<String, dynamic> json) {
    return Offer(
      id: json['id'] as String,
      title: json['title'] as String,
      description: json['description'] as String,
      type: json['type'] as int,
      isSeen: json['isSeen'] as bool? ?? false,
    );
  }

  // Fırsat türüne göre etiket ve renk (UI'da kullanacağız)
  String get typeLabel => switch (type) {
    0 => 'İçgörü',
    1 => 'Tasarruf',
    2 => 'Kampanya',
    3 => 'Trend',
    _ => 'Fırsat',
  };
}
