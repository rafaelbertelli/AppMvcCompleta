using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DevIO.App.ViewModels;
using DevIO.Business.Interfaces;
using AutoMapper;
using DevIO.Business.Models;

namespace DevIO.App.Controllers
{
    public class FornecedoresController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IFornecedorService _fornecedorService;
        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly INotificador _notificador;

        public FornecedoresController(
            IMapper mapper,
            IFornecedorService fornecedorService,
            IFornecedorRepository fornecedorRepository,
            INotificador notificador
        ) : base(notificador)
        {
            _mapper = mapper;
            _fornecedorService = fornecedorService;
            _fornecedorRepository = fornecedorRepository;
        }

        public async Task<IActionResult> Index()
        {
            List<Fornecedor> dbFornecedores = await _fornecedorRepository.ObterTodos();
            IEnumerable<FornecedorViewModel> fornecedores = _mapper.Map<IEnumerable<FornecedorViewModel>>(dbFornecedores);

            return View(fornecedores);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            FornecedorViewModel fornecedorViewModel = await ObterFornecedorEndereco(id);

            if (fornecedorViewModel == null)
                return NotFound();

            return View(fornecedorViewModel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FornecedorViewModel fornecedorViewModel)
        {
            if (!ModelState.IsValid)
                return View(fornecedorViewModel);

            Fornecedor fornecedor = _mapper.Map<Fornecedor>(fornecedorViewModel);
            await _fornecedorService.Adicionar(fornecedor);

            if (!OperacaoValida())
                return View(fornecedorViewModel);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            FornecedorViewModel fornecedorViewModel = await ObterFornecedorProdutosEndereco(id);
            
            if (fornecedorViewModel == null)
                return NotFound();

            return View(fornecedorViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, FornecedorViewModel fornecedorViewModel)
        {
            if (id != fornecedorViewModel.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(fornecedorViewModel);

            Fornecedor fornecedor = _mapper.Map<Fornecedor>(fornecedorViewModel);
            await _fornecedorService.Atualizar(fornecedor);

            if (!OperacaoValida())
                return View(fornecedorViewModel);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            FornecedorViewModel fornecedorViewModel = await ObterFornecedorEndereco(id);
            
            if (fornecedorViewModel == null)
                return NotFound();

            return View(fornecedorViewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            FornecedorViewModel fornecedorViewModel = await ObterFornecedorEndereco(id);

            if (fornecedorViewModel == null)
                return NotFound();

            await _fornecedorService.Remover(id);

            if (!OperacaoValida())
                return View(fornecedorViewModel);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AtualizarEndereco(Guid id)
        {
            var fornecedor = await ObterFornecedorEndereco(id);

            if (fornecedor == null)
                return NotFound();

            return PartialView("_AtualizarEndereco", 
                new FornecedorViewModel { Endereco = fornecedor.Endereco });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarEndereco(FornecedorViewModel fornecedorViewModel)
        {
            ModelState.Remove("Nome");
            ModelState.Remove("Documento");

            if (!ModelState.IsValid)
                return PartialView("_AtualizarEndereco", fornecedorViewModel);

            Endereco endereco = _mapper.Map<Endereco>(fornecedorViewModel.Endereco);

            await _fornecedorService.AtualizarEndereco(endereco);

            if (!OperacaoValida())
                return PartialView("_AtualizarEndereco", fornecedorViewModel);

            var url = Url.Action("ObterEndereco", "Fornecedores", new { id = fornecedorViewModel.Endereco.FornecedorId });

            return Json(new { success = true, url });
        }

        public async Task<IActionResult> ObterEndereco(Guid id)
        {
            var fornecedor = await ObterFornecedorEndereco(id);

            if (fornecedor == null)
                return NotFound();

            return PartialView("_DetalhesEndereco", fornecedor);
        }
        private async Task<FornecedorViewModel> ObterFornecedorEndereco(Guid id)
        {
            Fornecedor dbFornecedorEndereco = await _fornecedorRepository.ObterFornecedorEndereco(id);
            FornecedorViewModel fornecedorEndereco = _mapper.Map<FornecedorViewModel>(dbFornecedorEndereco);

            return fornecedorEndereco;
        }

        private async Task<FornecedorViewModel> ObterFornecedorProdutosEndereco(Guid id)
        {
            return _mapper.Map<FornecedorViewModel>(await _fornecedorRepository.ObterFornecedorProdutosEndereco(id));
        }
    }
}
