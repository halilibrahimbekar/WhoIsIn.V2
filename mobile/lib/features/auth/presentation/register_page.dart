import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/localization/app_localizations.dart';
import '../../../core/network/api_exception.dart';
import '../application/auth_controller.dart';

class RegisterPage extends ConsumerStatefulWidget {
  const RegisterPage({super.key});
  @override
  ConsumerState<RegisterPage> createState() => _RegisterPageState();
}

class _RegisterPageState extends ConsumerState<RegisterPage> {
  final _formKey = GlobalKey<FormState>();
  final _firstNameController = TextEditingController();
  final _lastNameController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  @override
  void dispose() { _firstNameController.dispose(); _lastNameController.dispose(); _emailController.dispose(); _passwordController.dispose(); super.dispose(); }
  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    await ref.read(authControllerProvider.notifier).register(email: _emailController.text.trim(), password: _passwordController.text, firstName: _firstNameController.text.trim(), lastName: _lastNameController.text.trim());
  }
  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context); final tr = strings.locale.languageCode == 'tr'; final authState = ref.watch(authControllerProvider);
    ref.listen(authControllerProvider, (previous, next) {
      if (next.error != null) { final message = next.error is ApiException ? (next.error as ApiException).message : (tr ? 'Kayıt başarısız oldu.' : 'Registration failed.'); ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message))); }
    });
    String required(String field) => tr ? '$field gerekli' : '$field is required';
    return Scaffold(appBar: AppBar(title: Text(strings.text('register')), actions: const [LanguageMenu()]), body: Center(child: ConstrainedBox(constraints: const BoxConstraints(maxWidth: 400), child: SingleChildScrollView(padding: const EdgeInsets.all(24), child: Form(key: _formKey, child: Column(mainAxisSize: MainAxisSize.min, children: [
      TextFormField(controller: _firstNameController, decoration: InputDecoration(labelText: tr ? 'Ad' : 'First name'), validator: (v) => v == null || v.isEmpty ? required(tr ? 'Ad' : 'First name') : null), const SizedBox(height: 12),
      TextFormField(controller: _lastNameController, decoration: InputDecoration(labelText: tr ? 'Soyad' : 'Last name'), validator: (v) => v == null || v.isEmpty ? required(tr ? 'Soyad' : 'Last name') : null), const SizedBox(height: 12),
      TextFormField(controller: _emailController, keyboardType: TextInputType.emailAddress, decoration: InputDecoration(labelText: tr ? 'E-posta' : 'Email'), validator: (v) => v == null || v.isEmpty ? required(tr ? 'E-posta' : 'Email') : null), const SizedBox(height: 12),
      TextFormField(controller: _passwordController, obscureText: true, decoration: InputDecoration(labelText: tr ? 'Şifre' : 'Password'), validator: (v) => v == null || v.length < 6 ? (tr ? 'Şifre en az 6 karakter olmalı' : 'Password must be at least 6 characters') : null), const SizedBox(height: 24),
      FilledButton(onPressed: authState.isLoading ? null : _submit, child: authState.isLoading ? const SizedBox(height: 16, width: 16, child: CircularProgressIndicator(strokeWidth: 2)) : Text(strings.text('register'))),
    ]))))));
  }
}
