import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/localization/app_localizations.dart';
import '../../../core/network/api_exception.dart';
import '../application/auth_controller.dart';

class LoginPage extends ConsumerStatefulWidget {
  const LoginPage({super.key});
  @override
  ConsumerState<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends ConsumerState<LoginPage> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();

  @override
  void dispose() { _emailController.dispose(); _passwordController.dispose(); super.dispose(); }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    await ref.read(authControllerProvider.notifier).login(email: _emailController.text.trim(), password: _passwordController.text);
  }

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    final tr = strings.locale.languageCode == 'tr';
    final authState = ref.watch(authControllerProvider);
    ref.listen(authControllerProvider, (previous, next) {
      if (next.error != null) {
        final message = next.error is ApiException ? (next.error as ApiException).message : (tr ? 'Giriş başarısız oldu.' : 'Sign in failed.');
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
      }
    });
    return Scaffold(
      appBar: AppBar(title: Text(strings.text('login')), actions: const [LanguageMenu()]),
      body: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 400),
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Form(
              key: _formKey,
              child: Column(mainAxisSize: MainAxisSize.min, children: [
          Text('WhoIsIn', style: Theme.of(context).textTheme.headlineMedium), const SizedBox(height: 24),
          TextFormField(controller: _emailController, keyboardType: TextInputType.emailAddress, decoration: InputDecoration(labelText: tr ? 'E-posta' : 'Email'), validator: (value) => value == null || value.isEmpty ? (tr ? 'E-posta gerekli' : 'Email is required') : null),
          const SizedBox(height: 12),
          TextFormField(controller: _passwordController, obscureText: true, decoration: InputDecoration(labelText: tr ? 'Şifre' : 'Password'), validator: (value) => value == null || value.isEmpty ? (tr ? 'Şifre gerekli' : 'Password is required') : null),
          const SizedBox(height: 24),
          FilledButton(onPressed: authState.isLoading ? null : _submit, child: authState.isLoading ? const SizedBox(height: 16, width: 16, child: CircularProgressIndicator(strokeWidth: 2)) : Text(strings.text('login'))),
          const SizedBox(height: 12),
          TextButton(onPressed: authState.isLoading ? null : () => context.push('/register'), child: Text(tr ? 'Hesabın yok mu? Kayıt ol' : 'No account? Create one')),
              ]),
            ),
          ),
        ),
      ),
    );
  }
}
