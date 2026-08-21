import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/network/api_exception.dart';
import '../application/events_providers.dart';
import '../data/events_repository.dart';
import '../models/event_models.dart';

class EventFormPage extends ConsumerStatefulWidget {
  const EventFormPage({super.key, this.existingEvent});

  final EventDetail? existingEvent;

  @override
  ConsumerState<EventFormPage> createState() => _EventFormPageState();
}

class _EventFormPageState extends ConsumerState<EventFormPage> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _titleController;
  late final TextEditingController _descriptionController;
  late final TextEditingController _capacityController;
  late final TextEditingController _locationController;
  late DateTime _startAt;
  String _visibility = 'Public';
  bool _isSaving = false;

  @override
  void initState() {
    super.initState();
    final existing = widget.existingEvent;
    _titleController = TextEditingController(text: existing?.title ?? '');
    _descriptionController = TextEditingController(text: existing?.description ?? '');
    _capacityController = TextEditingController(text: (existing?.capacity ?? 10).toString());
    _locationController = TextEditingController(text: existing?.locationName ?? '');
    _startAt = existing?.startAtUtc.toLocal() ?? DateTime.now().add(const Duration(days: 1));
    _visibility = existing?.visibility ?? 'Public';
  }

  @override
  void dispose() {
    _titleController.dispose();
    _descriptionController.dispose();
    _capacityController.dispose();
    _locationController.dispose();
    super.dispose();
  }

  Future<void> _pickStartDate() async {
    final date = await showDatePicker(
      context: context,
      initialDate: _startAt,
      firstDate: DateTime.now().subtract(const Duration(days: 1)),
      lastDate: DateTime.now().add(const Duration(days: 365 * 2)),
    );
    if (date == null || !mounted) return;
    final time = await showTimePicker(context: context, initialTime: TimeOfDay.fromDateTime(_startAt));
    if (time == null) return;
    setState(() {
      _startAt = DateTime(date.year, date.month, date.day, time.hour, time.minute);
    });
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _isSaving = true);
    final input = EventCreateInput(
      title: _titleController.text.trim(),
      description: _descriptionController.text.trim().isEmpty ? null : _descriptionController.text.trim(),
      visibility: _visibility,
      requireApproval: false,
      startAtUtc: _startAt.toUtc(),
      timeZone: DateTime.now().timeZoneName,
      locationName: _locationController.text.trim().isEmpty ? null : _locationController.text.trim(),
      capacity: int.tryParse(_capacityController.text) ?? 10,
    );

    try {
      final repository = ref.read(eventsRepositoryProvider);
      final existing = widget.existingEvent;
      final result = existing == null ? await repository.create(input) : await repository.update(existing.id, input);
      ref.invalidate(eventsListProvider);
      ref.invalidate(eventDetailProvider(result.id));
      if (!mounted) return;
      context.pop();
    } catch (error) {
      final message = error is ApiException ? error.message : 'Etkinlik kaydedilemedi.';
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final isEdit = widget.existingEvent != null;
    return Scaffold(
      appBar: AppBar(title: Text(isEdit ? 'Etkinligi Duzenle' : 'Yeni Etkinlik')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              TextFormField(
                controller: _titleController,
                decoration: const InputDecoration(labelText: 'Baslik'),
                validator: (value) => (value == null || value.isEmpty) ? 'Baslik gerekli' : null,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _descriptionController,
                decoration: const InputDecoration(labelText: 'Aciklama'),
                maxLines: 3,
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _locationController,
                decoration: const InputDecoration(labelText: 'Konum'),
              ),
              const SizedBox(height: 12),
              TextFormField(
                controller: _capacityController,
                decoration: const InputDecoration(labelText: 'Kapasite'),
                keyboardType: TextInputType.number,
                validator: (value) => (int.tryParse(value ?? '') == null) ? 'Gecerli bir sayi girin' : null,
              ),
              const SizedBox(height: 12),
              DropdownButtonFormField<String>(
                initialValue: _visibility,
                decoration: const InputDecoration(labelText: 'Gorunurluk'),
                items: const [
                  DropdownMenuItem(value: 'Public', child: Text('Public')),
                  DropdownMenuItem(value: 'InviteOnly', child: Text('InviteOnly')),
                ],
                onChanged: (value) => setState(() => _visibility = value ?? 'Public'),
              ),
              const SizedBox(height: 12),
              ListTile(
                contentPadding: EdgeInsets.zero,
                title: Text('Baslangic: ${_startAt.toString().substring(0, 16)}'),
                trailing: const Icon(Icons.edit_calendar),
                onTap: _pickStartDate,
              ),
              const SizedBox(height: 24),
              FilledButton(
                onPressed: _isSaving ? null : _submit,
                child: _isSaving
                    ? const SizedBox(height: 16, width: 16, child: CircularProgressIndicator(strokeWidth: 2))
                    : const Text('Kaydet'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
