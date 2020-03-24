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

    [Route("[controller]/[action]")]
    public class FornecedoresController : BaseController
    {
        private readonly IFornecedorRepository _context;
        private readonly IMapper _mapper;


        public FornecedoresController(
            IFornecedorRepository context,
            IMapper mapper
        )
        {
            _context = context;
            _mapper = mapper;
        }


        public async Task<IActionResult> Index()
        {
            var dbFornecedores = await _context.ObterTodos();
            var fornecedores = _mapper.Map<IEnumerable<FornecedorViewModel>>(dbFornecedores);

            return View(fornecedores);
        }


        public async Task<IActionResult> Details(Guid id)
        {
            var fornecedorViewModel = await ObterFornecedorEndereco(id);

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
        public async Task<IActionResult> Create([Bind("Id,Nome,Documento,TipoFornecedor,Ativo")] FornecedorViewModel fornecedorViewModel)
        {
            if (!ModelState.IsValid)
                return View(fornecedorViewModel);

            var fornecedor = _mapper.Map<Fornecedor>(fornecedorViewModel);
            await _context.Adicionar(fornecedor);

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(Guid id)
        {
            var fornecedorViewModel = await ObterFornecedorProdutosEndereco(id);
            
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

            var fornecedor = _mapper.Map<Fornecedor>(fornecedorViewModel);
            await _context.Atualizar(fornecedor);

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(Guid id)
        {
            var fornecedorViewModel = await ObterFornecedorEndereco(id);
            
            if (fornecedorViewModel == null)
                return NotFound();

            return View(fornecedorViewModel);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var fornecedorViewModel = await ObterFornecedorEndereco(id);

            if (fornecedorViewModel == null)
                return NotFound();

            await _context.Remover(id);

            return RedirectToAction(nameof(Index));
        }


        private async Task<FornecedorViewModel> ObterFornecedorEndereco(Guid id)
        {
            var dbFornecedorEndereco = await _context.ObterFornecedorEndereco(id);
            var fornecedorEndereco = _mapper.Map<FornecedorViewModel>(dbFornecedorEndereco);

            return fornecedorEndereco;
        }


        private async Task<FornecedorViewModel> ObterFornecedorProdutosEndereco(Guid id)
        {
            return _mapper.Map<FornecedorViewModel>(await _context.ObterFornecedorEndereco(id));
        }
    }
}
