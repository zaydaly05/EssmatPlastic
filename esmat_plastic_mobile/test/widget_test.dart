import 'package:flutter_test/flutter_test.dart';
import 'package:esmat_plastic_mobile/main.dart';

void main() {
  testWidgets('App smoke test', (WidgetTester tester) async {
    await tester.pumpWidget(const EsmatPlasticApp());
    expect(find.byType(EsmatPlasticApp), findsOneWidget);
  });
}
